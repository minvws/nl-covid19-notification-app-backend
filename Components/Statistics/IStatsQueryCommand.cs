namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    public interface IStatsQueryCommand
    {
        StatisticArgs Execute();
    }
}