using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Buyly.Application.DTOs.Payments;
using Buyly.Application.Exceptions;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Buyly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using DomainOrder = Buyly.Domain.Entities.Order;
using PayPalOrder = PayPalCheckoutSdk.Orders.Order;

namespace Buyly.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PayPalHttpClient _payPalClient;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(PayPalHttpClient payPalClient,
            AppDbContext context,
            ILogger<PaymentService> logger)
        {
            _payPalClient = payPalClient;
            _context = context;
            _logger = logger;
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentDto dto)
        {
            DomainOrder? order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
            {
                throw new NotFoundException("Order", dto.OrderId.ToString());
            }

            if (order.Status != OrderStatus.PendingPayment && order.Status != OrderStatus.PaymentFailed)
            {
                throw new ValidationException("Order is not eligible for a new payment intent.");
            }

            if (order.TotalPrice != dto.Amount)
            {
                throw new ValidationException("Payment amount does not match order total.");
            }

            var createRequest = new OrdersCreateRequest();
            createRequest.Prefer("return=representation");
            createRequest.RequestBody(BuildOrderRequest(dto));

            PayPalOrder? createdOrder;
            try
            {
                var response = await _payPalClient.Execute(createRequest);
                createdOrder = response.Result<PayPalOrder>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create PayPal order for OrderId {OrderId}", order.Id);
                throw new ValidationException("Unable to initiate payment with PayPal.");
            }

            if (createdOrder == null)
            {
                throw new ValidationException("PayPal did not return a valid order response.");
            }

            var approvalLink = createdOrder.Links?.FirstOrDefault(l =>
                l.Rel.Equals("approve", StringComparison.OrdinalIgnoreCase))?.Href;

            if (string.IsNullOrWhiteSpace(approvalLink))
            {
                throw new ValidationException("PayPal did not provide an approval link.");
            }

            var payment = order.Payments.FirstOrDefault(p =>
                p.Status.Equals("PendingApproval", StringComparison.OrdinalIgnoreCase));

            var isNewPayment = payment == null;

            if (payment == null)
            {
                payment = new Payment { OrderId = order.Id };
                _context.Payments.Add(payment);
            }

            payment.Provider = "PayPal";
            payment.PayPalOrderId = createdOrder.Id;
            payment.ApprovalUrl = approvalLink;
            payment.ApprovalExpiresAt = DateTime.UtcNow.AddMinutes(30);
            payment.Status = "PendingApproval";
            payment.Amount = dto.Amount;
            payment.Currency = dto.Currency;
            payment.TransactionId = string.Empty;
            payment.CapturedAt = DateTime.UtcNow;

            if (isNewPayment)
            {
                order.Payments.Add(payment);
            }

            order.Status = OrderStatus.PendingPayment;

            await _context.SaveChangesAsync();

            return new PaymentIntentDto
            {
                OrderId = order.Id,
                Provider = payment.Provider,
                Currency = payment.Currency,
                PayPalOrderId = createdOrder.Id,
                ApprovalUrl = approvalLink,
                ExpiresAt = payment.ApprovalExpiresAt
            };
        }

        public async Task<PaymentResultDto> CapturePaymentAsync(CapturePaymentDto dto)
        {
            DomainOrder? order = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (order == null)
            {
                throw new NotFoundException("Order", dto.OrderId.ToString());
            }

            if (order.Status != OrderStatus.PendingPayment && order.Status != OrderStatus.PaymentFailed)
            {
                throw new ValidationException("Order is not awaiting payment.");
            }

            var payment = order.Payments.FirstOrDefault(p =>
                p.PayPalOrderId == dto.PayPalOrderId);

            if (payment == null)
            {
                throw new ValidationException("Payment intent not found for this order.");
            }

            if (payment.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("This payment has already been captured.");
            }

            var captureRequest = new OrdersCaptureRequest(dto.PayPalOrderId);
            captureRequest.Prefer("return=representation");
            captureRequest.RequestBody(new OrderActionRequest());

            PayPalOrder? captureResult;
            try
            {
                var captureResponse = await _payPalClient.Execute(captureRequest);
                captureResult = captureResponse.Result<PayPalOrder>();
            }
            catch (Exception ex)
            {
                 Console.WriteLine(ex.Message);
                _logger.LogError(ex, "Failed to capture PayPal payment for OrderId {OrderId}", order.Id);
                throw new ValidationException("Unable to capture payment with PayPal.");
            }

            var capture = captureResult?.PurchaseUnits?.FirstOrDefault()?.Payments?.Captures?.FirstOrDefault();
            if (capture == null)
            {
                throw new ValidationException("PayPal capture response was invalid.");
            }

            DateTime capturedAt = DateTime.TryParse(capture.UpdateTime, out var parsedCapturedAt)
                ? parsedCapturedAt
                : DateTime.UtcNow;

            payment.TransactionId = capture.Id;
            payment.Status = capture.Status;
            payment.Amount = decimal.Parse(capture.Amount.Value, CultureInfo.InvariantCulture);
            payment.Currency = capture.Amount.CurrencyCode;
            payment.CapturedAt = capturedAt;
            payment.ApprovalUrl = null;
            payment.ApprovalExpiresAt = null;
            payment.PayerEmail = captureResult?.Payer?.Email;
            payment.PayerId = captureResult?.Payer?.PayerId;

            var success = capture.Status.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase);
            order.Status = success ? OrderStatus.Processing : OrderStatus.PaymentFailed;

            await _context.SaveChangesAsync();

            return new PaymentResultDto
            {
                Success = success,
                Message = capture.Status,
                TransactionId = capture.Id,
                Amount = payment.Amount,
                Currency = payment.Currency,
                CapturedAt = payment.CapturedAt
            };
        }

        public Task RefundPaymentAsync(string transactionId, decimal amount, string currency)
        {
            throw new NotImplementedException("Refunds are not implemented yet.");
        }

        private static OrderRequest BuildOrderRequest(CreatePaymentDto dto)
        {
            return new OrderRequest
            {
                CheckoutPaymentIntent = "CAPTURE",
                PurchaseUnits = new List<PurchaseUnitRequest>
                {
                    new PurchaseUnitRequest
                    {
                        ReferenceId = dto.OrderId.ToString(),
                        AmountWithBreakdown = new AmountWithBreakdown
                        {
                            CurrencyCode = dto.Currency,
                            Value = dto.Amount.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                },
                ApplicationContext = new ApplicationContext
                {
                    ReturnUrl = dto.ReturnUrl ?? "https://example.com/paypal/return",
                    CancelUrl = dto.CancelUrl ?? "https://example.com/paypal/cancel",
                    ShippingPreference = "NO_SHIPPING"
                }
            };
        }
    }
}
