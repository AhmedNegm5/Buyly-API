using System.Threading.Tasks;
using Buyly.API.Models;
using Buyly.Application.DTOs.Payments;
using Buyly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buyly.API.Controllers
{
    [Authorize]
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("capture")]
        [ProducesResponseType(typeof(ApiResponse<PaymentResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentDto dto)
        {
            var result = await _paymentService.CapturePaymentAsync(dto);
            return Success(result, "Payment captured successfully.");
        }

    }
}

