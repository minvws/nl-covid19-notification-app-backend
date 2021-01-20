using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup
{
    public class IksCleaningConfig : AppSettingsReader, IIksCleaningConfig
    {
        private const int DefaultLifetimeDays = 14;

        public IksCleaningConfig(IConfiguration config, string prefix = "Iks") : base(config, prefix)
        {
        }

        public int LifetimeDays => GetConfigValue(nameof(LifetimeDays), DefaultLifetimeDays);
    }
}
