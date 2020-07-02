namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public interface IServiceBusConfig
    {
        string QueueName { get; }
        string ConnectionString { get; }
    }
}