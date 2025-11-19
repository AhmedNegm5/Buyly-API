using Buyly.Application.DTOs.Payments;

namespace Buyly.API.Models.Requests
{
    public class RetryPaymentRequest
    {
        public PaymentInfoDto? Payment { get; set; }
    }
}

