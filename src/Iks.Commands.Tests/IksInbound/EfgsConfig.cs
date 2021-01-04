using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksInbound
{
    /// <summary>
    /// Stub for my config
    /// </summary>
    class EfgsConfig : IEfgsConfig
    {
        public string BaseUrl => "";
        public bool SendClientAuthenticationHeaders => true;
        public int DaysToDownload => 1;
        public int MaxBatchesPerRun => 10;
        public bool UploaderEnabled => true;
        public bool DownloaderEnabled => true;
    }
}