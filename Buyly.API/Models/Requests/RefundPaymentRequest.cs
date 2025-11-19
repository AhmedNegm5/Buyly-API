using System.ComponentModel.DataAnnotations;

namespace Buyly.API.Models.Requests
{
    public class RefundPaymentRequest
    {
        [Range(0.0, double.MaxValue)]
        public decimal? Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public string? Reason { get; set; }
    }
}

