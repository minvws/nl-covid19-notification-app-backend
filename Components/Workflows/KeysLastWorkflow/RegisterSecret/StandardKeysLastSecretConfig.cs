using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret
{
    public class StandardKeysLastSecretConfig : AppSettingsReader, IKeysLastSecretConfig
    {
        private static readonly IKeysLastSecretConfig _Defaults = new DefaultKeysLastSecretConfig();

        public StandardKeysLastSecretConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public int ByteCount => GetValueInt32(nameof(ByteCount), _Defaults.ByteCount);
    }
}