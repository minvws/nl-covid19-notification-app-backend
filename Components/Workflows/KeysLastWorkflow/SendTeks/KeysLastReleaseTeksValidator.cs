using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks
{
    public class KeysLastReleaseTeksValidator : IKeysLastReleaseTeksValidator
    {
        //TODO Seperate this concern...
        private readonly IKeysLastAuthorisationTokenValidator _AuthorisationTokenValidator;

        //TODO from this...
        private readonly IGeanTekListValidationConfig _Config;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;

        public KeysLastReleaseTeksValidator(IGeanTekListValidationConfig config, IKeysLastAuthorisationTokenValidator authorisationTokenValidator, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _Config = config;
            _AuthorisationTokenValidator = authorisationTokenValidator;
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator;
        }

        public bool Validate(KeysLastReleaseTeksArgs args)
        {
            if (args == null)
                return false;

            if (!_AuthorisationTokenValidator.Valid(args.Token))
                return false;

            if (_Config.TemporaryExposureKeyCountMin > args.Items.Length
                || args.Items.Length > _Config.TemporaryExposureKeyCountMax)
                return false;

            return args.Items.All(_TemporaryExposureKeyValidator.Valid);
        }
    }
}