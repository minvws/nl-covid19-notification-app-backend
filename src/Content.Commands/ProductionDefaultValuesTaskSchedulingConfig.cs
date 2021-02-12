namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class ProductionDefaultValuesTaskSchedulingConfig : ITaskSchedulingConfig
    {
        public double DailyCleanupHoursAfterMidnight => 5.0;
    }
}