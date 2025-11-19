using System.Threading.Tasks;
using Buyly.Application.DTOs.Payments;

namespace Buyly.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentIntentDto> CreatePaymentIntentAsync(CreatePaymentDto dto);
        Task<PaymentResultDto> CapturePaymentAsync(CapturePaymentDto dto);
        Task RefundPaymentAsync(string transactionId, decimal amount, string currency);
    }
}

