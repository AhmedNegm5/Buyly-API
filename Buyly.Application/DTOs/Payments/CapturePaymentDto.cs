using System;
using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.DTOs.Payments
{
    public class CapturePaymentDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        public string PayPalOrderId { get; set; } = string.Empty;
    }
}

