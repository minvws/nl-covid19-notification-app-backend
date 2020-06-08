// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks
{
    public class KeysFirstEscrowValidator : IKeysFirstEscrowValidator
    {
        private readonly IGeanTekListValidationConfig _Config;
        private readonly IKeysFirstAuthorisationTokenValidator _AuthorisationTokenValidator;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;

        public KeysFirstEscrowValidator(IGeanTekListValidationConfig config, IKeysFirstAuthorisationTokenValidator authorisationTokenValidator, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _Config = config;
            _AuthorisationTokenValidator = authorisationTokenValidator;
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator;
        }

        public bool Validate(KeysFirstEscrowArgs args)
        {
            if (args == null)
                return false;

            if (!_AuthorisationTokenValidator.IsValid(args.Token))
                return false;

            if (_Config.TemporaryExposureKeyCountMin > args.Items.Length
                || args.Items.Length > _Config.TemporaryExposureKeyCountMax)
                return false;

            return args.Items.All(_TemporaryExposureKeyValidator.Valid);
        }
    }
}