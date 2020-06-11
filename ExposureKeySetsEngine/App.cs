using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ExposureKeySetsEngine
{
    public class App
    {
        private readonly ILogger<App> _Logger;

        public App(ILogger<App> logger)
        {
            _Logger = logger;
        }

        public async Task Run()
        {
            _Logger.LogInformation("Running...");
            await Task.Delay(1000);
            _Logger.LogInformation("Completed...");
        }
    }
}
