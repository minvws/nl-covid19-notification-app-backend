// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class ReleaseTeksValidator : IReleaseTeksValidator
    {
        private readonly IGeanTekListValidationConfig _Config;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;

        public ReleaseTeksValidator(IGeanTekListValidationConfig config, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _Config = config;
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator;
        }

        public bool Validate(ReleaseTeksArgs args)
        {
            if (args == null)
                return false;

            if (!ResourceBundleValidator.IsBase64(args.BucketId))
                return false;

            if (_Config.TemporaryExposureKeyCountMin > args.Keys.Length
                || args.Keys.Length > _Config.TemporaryExposureKeyCountMax)
                return false;

            return args.Keys.All(_TemporaryExposureKeyValidator.Valid);
        }
    }
}