using System;

namespace Buyly.Application.DTOs.Payments
{
    public class RefundResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RefundId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime? RefundedAt { get; set; }
    }
}

