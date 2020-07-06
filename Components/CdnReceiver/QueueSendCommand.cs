using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class QueueSendCommand<T>
    {
        private readonly IServiceBusConfig _SbConfig;
        private readonly IJsonSerializer _JsonSerializer;

        public QueueSendCommand(IServiceBusConfig sbConfig, IJsonSerializer jsonSerializer)
        {
            _SbConfig = sbConfig;
            _JsonSerializer = jsonSerializer;
        }

        public async Task Execute(T args)
        {
            var queueClient = new QueueClient(_SbConfig.ConnectionString, _SbConfig.QueueName);
            try
            {
                var m = new Message(Encoding.UTF8.GetBytes(_JsonSerializer.Serialize(args)));
                await queueClient.SendAsync(m);
            }
            finally
            {
                await queueClient?.CloseAsync();
            }
        }
    }
}