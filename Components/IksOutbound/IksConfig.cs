using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound
{
    public class IksConfig : AppSettingsReader, IIksConfig
    {
        public IksConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public int ItemCountMax => GetConfigValue(nameof(ItemCountMax), 750000); 
        public int PageSize => GetConfigValue(nameof(PageSize), 10000);
    }
}