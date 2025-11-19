using System;

namespace Buyly.Application.DTOs.Payments
{
    public class PaymentIntentDto
    {
        public Guid OrderId { get; set; }
        public string Provider { get; set; } = "PayPal";
        public string Currency { get; set; } = "USD";
        public string PayPalOrderId { get; set; } = string.Empty;
        public string ApprovalUrl { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}

