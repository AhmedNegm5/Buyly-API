using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.DTOs.Payments
{
    public class RefundPaymentDto
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Range(0.0, double.MaxValue)]
        public decimal? Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public string? Reason { get; set; }
    }
}

