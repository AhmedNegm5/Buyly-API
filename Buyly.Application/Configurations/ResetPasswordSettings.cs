using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.Configurations
{
    public class ResetPasswordSettings
    {
        [Range(0, 3600)]
        public int CooldownSeconds { get; set; } = 60;

        [Range(0, 1440)]
        public int WindowMinutes { get; set; } = 60;

        [Range(0, 100)]
        public int WindowLimit { get; set; } = 5;
    }
}

