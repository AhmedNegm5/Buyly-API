namespace Buyly.Domain.Entities;

public class Order : BaseEntity
{
    public string UserId{ get; set; } = null!;
    public User User { get; set; } = null!;
    
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PendingPayment;

    public decimal TotalPrice { get; set; }
    public string ShippingAddress { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}


