using System.ComponentModel.DataAnnotations;

namespace Buyly.Infrastructure.Configurations
{
    public class EmailSettings
    {
        [Required]
        public string SmtpHost { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        [Required]
        [EmailAddress]
        public string SenderEmail { get; set; } = string.Empty;

        [Required]
        public string SenderName { get; set; } = "Buyly Support";

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

