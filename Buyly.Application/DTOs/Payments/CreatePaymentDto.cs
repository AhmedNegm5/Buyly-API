using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.DTOs.Payments
{
    public class CreatePaymentDto
    {
        [Required]
        public Guid OrderId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "USD";

        public string? ReturnUrl { get; set; }

        public string? CancelUrl { get; set; }
    }
}

