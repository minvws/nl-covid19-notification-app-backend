using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class ManifestConfig : AppSettingsReader, IManifestConfig
    {
        public ManifestConfig(IConfiguration config, string? prefix = "Manifest") : base(config, prefix)
        {
        }

        public int KeepAliveCount => GetConfigValue(nameof(KeepAliveCount), 5);
    }
}