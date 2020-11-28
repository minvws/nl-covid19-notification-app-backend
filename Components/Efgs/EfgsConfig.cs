using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs
{
    public class EfgsConfig : AppSettingsReader, IEfgsConfig
    {
        public EfgsConfig(IConfiguration config, string? prefix = "Efgs") : base(config, prefix) { }
        public string BaseUrl => GetConfigValue(nameof(BaseUrl), "http://localhost:8080");
        public bool SendClientAuthenticationHeaders => GetConfigValue(nameof(SendClientAuthenticationHeaders), false);
        public int DaysToDownload => GetConfigValue(nameof(DaysToDownload), 7);
        public int MaxBatchesPerRun => GetConfigValue(nameof(MaxBatchesPerRun), 10);
        public bool UploaderEnabled => GetConfigValue(nameof(UploaderEnabled), true);
        public bool DownloaderEnabled => GetConfigValue(nameof(DownloaderEnabled), true);
    }
}