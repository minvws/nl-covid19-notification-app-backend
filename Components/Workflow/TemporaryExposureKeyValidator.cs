// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public class TemporaryExposureKeyValidator : ITemporaryExposureKeyValidator
    {
        private readonly ITemporaryExposureKeyValidatorConfig _Config;
        private readonly ILogger<ReleaseTeksValidator> _Logger;

        public TemporaryExposureKeyValidator(ITemporaryExposureKeyValidatorConfig config, ILogger<ReleaseTeksValidator> logger)
        {
            _Config = config;
            _Logger = logger;
        }

        public bool Valid(TemporaryExposureKeyArgs value)
        {
            if (value == null)
            {
                _Logger.LogWarning("Tek is null");
                return false;
            }

            if (_Config.RollingPeriodMin > value.RollingPeriod || value.RollingPeriod > _Config.RollingPeriodMax)
            {
                _Logger.LogWarning("Tek RollingPeriod out of range.");
                return false;
            }

            //TODO valid values epoch size, currently 10mins, for value.RollingStartNumber 

            if (string.IsNullOrEmpty(value.KeyData))
            {
                _Logger.LogWarning("Tek keydata is empty.");
                return false;
            }

            try
            {
                //GUARANTEES a successful conversion at the point of creating the exposure key set
                //by using the same function in both instances
                var bytes = Convert.FromBase64String(value.KeyData);

                if (bytes.Length != _Config.DailyKeyByteCount)
                {
                    _Logger.LogWarning("Tek keydata wrong length.");
                    return false;
                }
            }
            catch (FormatException)
            {
                _Logger.LogWarning("Tek keydata not valid.");
                return false;
            }

            return true;
        }
    }
}