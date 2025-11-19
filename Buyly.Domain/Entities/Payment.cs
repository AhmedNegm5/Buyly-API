namespace Buyly.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public string Provider { get; set; } = "PayPal";
        public string PayPalOrderId { get; set; } = null!;
        public string? ApprovalUrl { get; set; }
        public DateTime? ApprovalExpiresAt { get; set; }
        public string TransactionId { get; set; } = null!;
        public string Status { get; set; } = "Pending";
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
        public string? PayerEmail { get; set; }
        public string? PayerId { get; set; }
    }
}

