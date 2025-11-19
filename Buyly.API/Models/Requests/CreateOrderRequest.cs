using Buyly.Application.DTOs.Payments;

namespace Buyly.API.Models.Requests
{
public class CreateOrderRequest
{
    public PaymentInfoDto Payment { get; set; } = new();
}
}

