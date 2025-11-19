using System.ComponentModel.DataAnnotations;

namespace Buyly.Infrastructure.Configurations
{
    public class PayPalSettings
    {
        [Required]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        public string ClientSecret { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Sandbox|Live", ErrorMessage = "Environment must be either 'Sandbox' or 'Live'.")]
        public string Environment { get; set; } = "Sandbox"; // or Live
    }
}

