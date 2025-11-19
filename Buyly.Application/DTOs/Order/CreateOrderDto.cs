using Buyly.Application.DTOs.Payments;

namespace Buyly.Application.DTOs.Order;

public class CreateOrderDto
{
    public List<OrderItemDto> Items { get; set; } = new();
    public PaymentInfoDto Payment { get; set; } = new();
}