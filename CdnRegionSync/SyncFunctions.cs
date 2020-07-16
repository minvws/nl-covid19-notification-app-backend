using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CdnRegionSync
{
    public class SyncFunctions : AppFunctionBase
    {
        const string Source = "Source";
        const string Destination = "Destination";
        
        //NB this is for Storage Queues not MBQ. Confirm behaviour is the same.
        //https://stackoverflow.com/questions/48456580/azure-functions-with-service-bus-how-to-keep-a-message-in-the-queue-if-somethin
        [FunctionName("ReceiveSyncMessage")]
        public async Task Run([ServiceBusTrigger("%QueueName%", Connection = "QueueConnectionString", IsSessionsEnabled = true)]
            Message message,
            ExecutionContext executionContext,
            ILogger logger
            )
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (executionContext == null) throw new ArgumentNullException(nameof(executionContext));

            logger.LogInformation($"Triggered.");
            SetConfig(executionContext);

            var json = Encoding.UTF8.GetString(message.Body);
            var args = new StandardJsonSerializer().Deserialize<StorageAccountSyncMessage>(json);
            
            logger.LogDebug($"Message Id:{message.MessageId} Contents: {json}.");

            if (args.MutableContent)
                await new MutableBlobCopyCommand(new StorageAccountAppSettings(Configuration, Source), new StorageAccountAppSettings(Configuration, Destination), logger)
                    .Execute(args.RelativePath);
            else    
                await new ImmutableBlobCopyCommand(new StorageAccountAppSettings(Configuration, Source), new StorageAccountAppSettings(Configuration, Destination), logger)
                    .Execute(args.RelativePath);
        }
    }
}
