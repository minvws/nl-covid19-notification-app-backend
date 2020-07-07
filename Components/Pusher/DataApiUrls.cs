using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class DataApiUrls : AppSettingsReader, IDataApiUrls
    {
        public DataApiUrls(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string Username => GetValue(nameof(Username));
        public string Password => GetValue(nameof(Password));
        public string Manifest => GetValue(nameof(Manifest));
        public string AppConfig => GetValue(nameof(AppConfig));
        public string ExposureKeySet => GetValue(nameof(ExposureKeySet));
        public string RiskCalculationParameters => GetValue(nameof(RiskCalculationParameters));
        public string ResourceBundle => GetValue(nameof(ResourceBundle));
    }
}