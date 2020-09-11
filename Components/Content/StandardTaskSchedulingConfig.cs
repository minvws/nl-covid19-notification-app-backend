using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class StandardTaskSchedulingConfig : AppSettingsReader, ITaskSchedulingConfig
    {
        public StandardTaskSchedulingConfig(IConfiguration config, string? prefix = "TaskScheduling") : base(config, prefix)
        {
        }

        public double DailyCleanupHoursAfterMidnight => GetConfigValue(nameof(DailyCleanupHoursAfterMidnight), 5.0);
    }
}