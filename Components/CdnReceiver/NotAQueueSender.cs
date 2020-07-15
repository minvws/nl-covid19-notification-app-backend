using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class NotAQueueSender<T> : IQueueSender<T>
    {
        private readonly ILogger _Logger;

        public NotAQueueSender(ILogger logger)
        {
            _Logger = logger;
        }

        public Task Send(T message)
        {
            //Not a sausage.
            _Logger.LogWarning("Message not queued - NotAQueueSender active.");
            return Task.CompletedTask;
        }
    }
}