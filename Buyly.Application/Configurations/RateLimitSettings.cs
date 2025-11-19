using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.Configurations
{
    public class RateLimitSettings
    {
        [Required]
        public RateLimitRule GlobalLimit { get; set; } = new();

        [Required]
        public RateLimitRule StrictLimit { get; set; } = new();
    }

    public class RateLimitRule
    {
        [Range(1, int.MaxValue)]
        public int PermitLimit { get; set; } = 200;

        [Range(1, int.MaxValue)]
        public int WindowSeconds { get; set; } = 60;

        [Range(0, int.MaxValue)]
        public int QueueLimit { get; set; } = 0;
    }
}

