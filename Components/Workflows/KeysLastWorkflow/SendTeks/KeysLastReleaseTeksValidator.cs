using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class KeysLastReleaseTeksValidator : IKeysLastReleaseTeksValidator
    {
        private readonly IGeanTekListValidationConfig _Config;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;

        public KeysLastReleaseTeksValidator(IGeanTekListValidationConfig config, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _Config = config;
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator;
        }

        public bool Validate(KeysLastReleaseTeksArgs args)
        {
            if (args == null)
                return false;

            if (!ResourceBundleValidator.IsBase64(args.BucketId))
                return false;

            if (_Config.TemporaryExposureKeyCountMin > args.Items.Length
                || args.Items.Length > _Config.TemporaryExposureKeyCountMax)
                return false;

            return args.Items.All(_TemporaryExposureKeyValidator.Valid);
        }
    }
}