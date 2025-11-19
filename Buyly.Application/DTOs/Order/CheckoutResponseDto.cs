using Buyly.Application.DTOs.Payments;

namespace Buyly.Application.DTOs.Order
{
    public class CheckoutResponseDto
    {
        public OrderDto Order { get; set; } = null!;
        public PaymentIntentDto PaymentIntent { get; set; } = null!;
    }
}

