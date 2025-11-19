using System.ComponentModel.DataAnnotations;

namespace Buyly.Application.Configurations
{
    public class CleanupSettings
    {
        [Range(1, 1440)]
        public int ExecutionIntervalMinutes { get; set; } = 60;

        [Range(0.1, 168)]
        public double PendingOrderMaxAgeHours { get; set; } = 24;

        [Range(0.1, 168)]
        public double RestoredCartMaxAgeHours { get; set; } = 48;
    }
}

