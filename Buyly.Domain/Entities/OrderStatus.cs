namespace Buyly.Domain.Entities;

public enum OrderStatus
{
    PendingPayment = 0,
    Pending = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    PaymentFailed = 6
}