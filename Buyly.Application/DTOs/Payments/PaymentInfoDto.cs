using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.DTOs.Payments
{
    public class PaymentInfoDto
    {
        [Required]
        public string Method { get; set; } = "PayPal";

        [Required]
        public string Currency { get; set; } = "USD";

        public string? ReturnUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}

