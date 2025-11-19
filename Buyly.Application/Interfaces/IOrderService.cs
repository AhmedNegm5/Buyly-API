using Buyly.Application.DTOs.Order;
using Buyly.Application.DTOs.Payments;

namespace Buyly.Application.Interfaces;

public interface IOrderService
{
    Task<CheckoutResponseDto> CreateOrderAsync(string userId, CreateOrderDto dto);
    Task<OrderDto> CaptureOrderPaymentAsync(string userId, Guid orderId, string payPalOrderId);
    Task<CheckoutResponseDto> RetryOrderPaymentAsync(string userId, Guid orderId, PaymentInfoDto? paymentInfo);
    Task<OrderDto?> GetOrderByIdAsync(Guid orderId);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(string userId);
    Task<bool> UpdateOrderStatusAsync(Guid orderId, string status);
    Task<bool> DeleteOrderAsync(Guid orderId);
}