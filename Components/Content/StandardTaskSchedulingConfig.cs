using System;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class StandardTaskSchedulingConfig : AppSettingsReader, ITaskSchedulingConfig
    {
        public StandardTaskSchedulingConfig(IConfiguration config, string? prefix = "TaskScheduling") : base(config, prefix)
        {
        }

        public double DailyCleanupHoursAfterMidnight
        {
            get
            {
                var raw = GetConfigValue("DailyCleanupStartTime", "05:00:00");
                var result = DateTime.ParseExact(raw, "hh:mm:ss", CultureInfo.InvariantCulture);
                return result.TimeOfDay.TotalHours;
            }
        }
    }
}