using Buyly.Application.DTOs.Order;
using Buyly.Application.DTOs.Payments;
using Buyly.Application.Exceptions;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Buyly.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly UserManager<User> _userManager;
    private readonly IPaymentService _paymentService;
    private readonly ICartService _cartService;
    
    public OrderService(IUnitOfWork unitOfWork,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        UserManager<User> userManager,
        IPaymentService paymentService,
        ICartService cartService)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _userManager = userManager;
        _paymentService = paymentService;
        _cartService = cartService;
    }
    public async Task<CheckoutResponseDto> CreateOrderAsync(string userId, CreateOrderDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(user.Address))
        {
            throw new ValidationException("User must have a saved shipping address before placing an order.");
        }

        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            throw new ValidationException("User must have a phone number on file before placing an order.");
        }
        
        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new ValidationException("Order must contain at least one item.");
        }

        if (dto.Items.Any(i => i.Quantity <= 0))
        {
            throw new ValidationException("Each order item must have a quantity greater than zero.");
        }

        var groupedItems = dto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(item => item.Quantity)
            })
            .ToList();

        var productDictionary = await _productRepository.GetProductsByIdsAsync(groupedItems.Select(g => g.ProductId));
        if (productDictionary.Count != groupedItems.Count)
        {
            var missingProductId = groupedItems.First(g => !productDictionary.ContainsKey(g.ProductId)).ProductId;
            throw new NotFoundException("Product", missingProductId.ToString());
        }

        var now = DateTime.UtcNow;

        foreach (var item in groupedItems)
        {
            var product = productDictionary[item.ProductId];
            if (product.Stock < item.Quantity)
            {
                throw new ValidationException($"Insufficient stock for product '{product.Name}'.");
            }

            product.Stock -= item.Quantity;
            product.UpdatedAt = now;
            _unitOfWork.Repository<Product>().Update(product);
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = now,
            Status = OrderStatus.PendingPayment,
            PhoneNumber = user.PhoneNumber!,
            ShippingAddress = user.Address!,
            OrderItems = new List<OrderItem>()
        };

        foreach (var dtoItem in dto.Items)
        {
            var product = productDictionary[dtoItem.ProductId];
            var orderItem = new OrderItem
            {
                ProductId = dtoItem.ProductId,
                Quantity = dtoItem.Quantity,
                UnitPrice = product.Price
            };
            order.OrderItems.Add(orderItem);
        }
        
        order.TotalPrice = order.OrderItems.Sum(oi => oi.Quantity * oi.UnitPrice);


        await _orderRepository.AddAsync(order);
        await _unitOfWork.CommitAsync();

        var paymentInfo = dto.Payment ?? new PaymentInfoDto();
        var paymentMethod = paymentInfo.Method ?? "PayPal";
        var currency = string.IsNullOrWhiteSpace(paymentInfo.Currency) ? "USD" : paymentInfo.Currency;

        if (!string.Equals(paymentMethod, "PayPal", StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Currently only PayPal payments are supported.");
        }

        var intent = await _paymentService.CreatePaymentIntentAsync(new CreatePaymentDto
        {
            OrderId = order.Id,
            Amount = order.TotalPrice,
            Currency = currency,
            ReturnUrl = paymentInfo.ReturnUrl,
            CancelUrl = paymentInfo.CancelUrl
        });

        var refreshedOrder = await _orderRepository.GetOrderWithDetailsAsync(order.Id) ?? order;

        return new CheckoutResponseDto
        {
            Order = MapToOrderDto(refreshedOrder),
            PaymentIntent = intent
        };
    }

    public async Task<OrderDto> CaptureOrderPaymentAsync(string userId, Guid orderId, string payPalOrderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId.ToString());

        if (!order.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("You are not allowed to capture payment for this order.");
        }

        var result = await _paymentService.CapturePaymentAsync(new CapturePaymentDto
        {
            OrderId = orderId,
            PayPalOrderId = payPalOrderId
        });

        if (!result.Success)
        {
            await RestockProductsAsync(order.OrderItems);
            await _cartService.RestoreCartFromOrderAsync(userId, order.OrderItems);
            throw new ValidationException($"Payment failed: {result.Message}");
        }

        var refreshedOrder = await _orderRepository.GetOrderWithDetailsAsync(orderId) ?? order;

        return MapToOrderDto(refreshedOrder);
    }

    public async Task<CheckoutResponseDto> RetryOrderPaymentAsync(string userId, Guid orderId, PaymentInfoDto? paymentInfo)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId)
            ?? throw new NotFoundException("Order", orderId.ToString());

        if (!order.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("You are not allowed to retry payment for this order.");
        }

        if (order.Status != OrderStatus.PendingPayment && order.Status != OrderStatus.PaymentFailed)
        {
            throw new ValidationException("Only pending or failed orders can be retried.");
        }

        var reusablePayment = order.Payments
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefault(p =>
                p.Status.Equals("PendingApproval", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(p.ApprovalUrl) &&
                (!p.ApprovalExpiresAt.HasValue || p.ApprovalExpiresAt > DateTime.UtcNow));

        PaymentIntentDto paymentIntent;

        if (reusablePayment != null)
        {
            paymentIntent = MapPaymentToIntent(reusablePayment);
        }
        else
        {
            var fallbackCurrency = order.Payments
                .OrderByDescending(p => p.CreatedAt)
                .FirstOrDefault()?.Currency ?? "USD";

            var resolvedCurrency = !string.IsNullOrWhiteSpace(paymentInfo?.Currency)
                ? paymentInfo!.Currency
                : fallbackCurrency;

            paymentIntent = await _paymentService.CreatePaymentIntentAsync(new CreatePaymentDto
            {
                OrderId = order.Id,
                Amount = order.TotalPrice,
                Currency = resolvedCurrency,
                ReturnUrl = paymentInfo?.ReturnUrl,
                CancelUrl = paymentInfo?.CancelUrl
            });
        }

        return new CheckoutResponseDto
        {
            Order = MapToOrderDto(order),
            PaymentIntent = paymentIntent
        };
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId)
    {
        var order = await _orderRepository.GetOrderWithDetailsAsync(orderId);
        
        return order == null ? null : MapToOrderDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId)
    {
        var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
        
        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            return false;

        if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            return false;
        
        order.Status = orderStatus;
        _orderRepository.Update(order);
        await _unitOfWork.CommitAsync();
        return true;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if(order == null) return false;
        _orderRepository.Delete(order);
        await _unitOfWork.CommitAsync();
        return true;
    }
    private static PaymentIntentDto MapPaymentToIntent(Payment payment)
    {
        return new PaymentIntentDto
        {
            OrderId = payment.OrderId,
            Provider = payment.Provider,
            Currency = payment.Currency,
            PayPalOrderId = payment.PayPalOrderId,
            ApprovalUrl = payment.ApprovalUrl ?? string.Empty,
            ExpiresAt = payment.ApprovalExpiresAt
        };
    }

    private OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString(),
            TotalPrice = order.TotalPrice,
            OrderItems = order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };
    }

    private async Task RestockProductsAsync(IEnumerable<OrderItem> orderItems)
    {
        var grouped = orderItems
            .GroupBy(item => item.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                Quantity = g.Sum(item => item.Quantity)
            })
            .ToList();

        if (!grouped.Any())
        {
            return;
        }

        foreach (var entry in grouped)
        {
            var product = await _productRepository.GetByIdAsync(entry.ProductId);
            if (product == null)
            {
                continue;
            }

            product.Stock += entry.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Product>().Update(product);
        }

        await _unitOfWork.CommitAsync();
    }
}