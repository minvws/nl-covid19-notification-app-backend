using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using Serilog.Settings.Configuration;

namespace CdnRegionSync
{
    public class SyncFunctions : AppFunctionBase
    {
        //NB this is for Storage Queues not MBQ. Confirm behaviour is the same.
        //https://stackoverflow.com/questions/48456580/azure-functions-with-service-bus-how-to-keep-a-message-in-the-queue-if-somethin
        [FunctionName("ReceiveSyncMessage")]
        public async Task Run([ServiceBusTrigger("vwspt-queue", Connection = "Queue", IsSessionsEnabled = true)]
            Message message,
            ExecutionContext executionContext
            )
        {
            SetConfig(executionContext);
            var json = Encoding.UTF8.GetString(message.Body);
            var args = new StandardJsonSerializer().Deserialize<StorageAccountSyncMessage>(json);

            if (args.MutableContent)
                await new MutableBlobCopyCommand(new StorageAccountAppSettings(Configuration, "Source"), new StorageAccountAppSettings(Configuration, "Destination"))
                    .Execute(args.RelativePath);
            else    
                await new ImmutableBlobCopyCommand(new StorageAccountAppSettings(Configuration, "Source"), new StorageAccountAppSettings(Configuration, "Destination"))
                    .Execute(args.RelativePath);
        }
    }
}
