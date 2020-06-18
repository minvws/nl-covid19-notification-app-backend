// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TemporaryExposureKeyValidator : ITemporaryExposureKeyValidator
    {
        private readonly ITemporaryExposureKeyValidatorConfig _Config;

        public TemporaryExposureKeyValidator(ITemporaryExposureKeyValidatorConfig config)
        {
            _Config = config;
        }

        public bool Valid(TemporaryExposureKeyArgs value)
        {
            if (value == null)
                return false;

            if (_Config.RollingPeriodMin > value.RollingPeriod || value.RollingPeriod > _Config.RollingPeriodMax)
                return false;

            //TODO valid values epoch size, currently 10mins, for value.RollingStartNumber 

            if (string.IsNullOrEmpty(value.KeyData))
                return false;

            try
            {
                //GUARANTEES a successful conversion at the point of creating the exposure key set
                //by using the same function in both instances
                var bytes = Convert.FromBase64String(value.KeyData);

                if (bytes.Length != _Config.DailyKeyByteCount)
                    return false;
            }
            catch (FormatException)
            {
                return false;
            }

            return true;
        }
    }
}