using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class ManifestMaxAgeCalculator
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ITaskSchedulingConfig _TaskSchedulingConfig;

        public ManifestMaxAgeCalculator(IUtcDateTimeProvider dateTimeProvider, ITaskSchedulingConfig taskSchedulingConfig)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _TaskSchedulingConfig = taskSchedulingConfig ?? throw new ArgumentNullException(nameof(taskSchedulingConfig));
        }

        public int Execute(DateTime created)
        {
            var life = _DateTimeProvider.Snapshot - created;
            var remaining = (int)Math.Floor(TimeSpan.FromMinutes(_TaskSchedulingConfig.ManifestPeriodMinutes).TotalSeconds - life.TotalSeconds);
            return Math.Max(remaining, 60); //Give it another minute in case the ManifestEngine is late.
        }
    }
}