using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs
{
    public class EfgsConfig : AppSettingsReader, IEfgsConfig
    {
        public EfgsConfig(IConfiguration config, string? prefix = "Efgs") : base(config, prefix) { }
        public string BaseUrl => GetConfigValue(nameof(BaseUrl), "http://localhost:8080");
        public bool SendClientAuthenticationHeaders => GetConfigValue(nameof(SendClientAuthenticationHeaders), false);
    }
}