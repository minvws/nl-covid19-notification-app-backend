using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentPusherEngine
{
    public class ReceiverConfig : AppSettingsReader, IReceiverConfig
    {
        public ReceiverConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string Username { get; }
        public string Password { get; }
        public string Manifest => GetValue(nameof(Manifest));
        public string AppConfig => GetValue(nameof(AppConfig));
        public string ExposureKeySet => GetValue(nameof(ExposureKeySet));
        public string RiskCalculationParameters => GetValue(nameof(RiskCalculationParameters));
        public string ResourceBundle => GetValue(nameof(ResourceBundle));
    }
}