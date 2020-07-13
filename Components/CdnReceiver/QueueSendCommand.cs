using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class QueueSendCommand<T> : IQueueSender<T>
    {
        private readonly IServiceBusConfig _SbConfig;
        private readonly IJsonSerializer _JsonSerializer;
        private readonly ILogger _Logger;

        public QueueSendCommand(IServiceBusConfig sbConfig, IJsonSerializer jsonSerializer, ILogger logger)
        {
            _SbConfig = sbConfig ?? throw new ArgumentNullException(nameof(sbConfig));
            _JsonSerializer = jsonSerializer ?? throw new ArgumentNullException(nameof(jsonSerializer));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Send(T args)
        {
            var queueClient = new QueueClient(_SbConfig.ConnectionString, _SbConfig.QueueName);
            _Logger.Debug($"Writing to queue - Endpoint:{queueClient.ServiceBusConnection.Endpoint}, {queueClient.Path}.");
            try
            {
                var m = new Message(Encoding.UTF8.GetBytes(_JsonSerializer.Serialize(args)));
                m.SessionId = Guid.NewGuid().ToString();
                await queueClient.SendAsync(m);
                _Logger.Information("Items written to queue.");
            }
            finally
            {
                await queueClient?.CloseAsync();
            }
        }
    }
}