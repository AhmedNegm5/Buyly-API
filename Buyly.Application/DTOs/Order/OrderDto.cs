namespace Buyly.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItemDto> OrderItems { get; set; } = new();
}