using System.ComponentModel.DataAnnotations;

namespace Buyly.API.Models.Requests
{
public class CaptureOrderRequest
{
    [Required]
    public string PayPalOrderId { get; set; } = string.Empty;
}
}

