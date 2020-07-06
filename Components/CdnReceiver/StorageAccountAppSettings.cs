using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnDataReceiver
{
    public class StorageAccountAppSettings : AppSettingsReader, IStorageAccountConfig
    {
        public StorageAccountAppSettings(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string ConnectionString => GetValue("ConnectionString");
    }
}