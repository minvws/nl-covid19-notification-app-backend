using System;
using System.Collections.Generic;
using System.Text;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services
{
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;
    using Newtonsoft.Json;

    namespace PrivateTracer.Server.Azure.Components.Services
    {
        public class QueueSendCommand<T>
        {
            private readonly IServiceBusConfig _SbConfig;

            public QueueSendCommand(IServiceBusConfig sbConfig)
            {
                _SbConfig = sbConfig;
            }

            public async Task Execute(T args)
            {
                var queueClient = new QueueClient(_SbConfig.ConnectionString, _SbConfig.QueueName);
                try
                {
                    var m = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(args)));
                    await queueClient.SendAsync(m);
                }
                finally
                {
                    await queueClient?.CloseAsync();
                }
            }
        }
    }
}
