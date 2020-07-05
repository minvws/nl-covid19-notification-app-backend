using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public interface IServiceBusConfig
    {
        string QueueName { get; }
        string ConnectionString { get; }
    }

    public class ServiceBusConfig : AppSettingsReader, IServiceBusConfig
    {
        public ServiceBusConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string QueueName => GetValue(nameof(QueueName));
        public string ConnectionString => GetValue(nameof(ConnectionString));
    }
}