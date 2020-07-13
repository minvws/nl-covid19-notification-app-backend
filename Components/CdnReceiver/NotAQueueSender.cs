using System.Threading.Tasks;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class NotAQueueSender<T> : IQueueSender<T>
    {
        public Task Send(T message)
        {
            //Not a sausage.
            Log.Warning("Message not queued - NotAQueueSender active.");
            return Task.CompletedTask;
        }
    }
}