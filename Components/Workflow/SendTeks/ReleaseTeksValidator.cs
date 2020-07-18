// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public class ReleaseTeksValidator : IReleaseTeksValidator
    {
        private readonly IGeanTekListValidationConfig _Config;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;
        private readonly ILogger _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public ReleaseTeksValidator(
            IGeanTekListValidationConfig config, 
            ITemporaryExposureKeyValidator temporaryExposureKeyValidator, 
            ILogger<ReleaseTeksValidator> logger, 
            IUtcDateTimeProvider dateTimeProvider)
        {
            _Config = config;
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator;
            _Logger = logger;
            _DateTimeProvider = dateTimeProvider;
        }

        public bool Validate(ReleaseTeksArgs args, KeyReleaseWorkflowState workflow)
        {
            if (args == null)
                return false;

            if (workflow.ValidUntil.AddMinutes(_Config.GracePeriodMinutes) <= _DateTimeProvider.Now()) //30 minutes grace period
            {
                _Logger.LogWarning($"Workflow is not valid anymore: {args.BucketId}");
                return false;
            }
            
            if (_Config.TemporaryExposureKeyCountMin > args.Keys.Length || args.Keys.Length > _Config.TemporaryExposureKeyCountMax)
            {
                _Logger.LogWarning($"Invalid number of keys: {args.BucketId}");
                return false;
            }

            if (!args.Keys.All(_TemporaryExposureKeyValidator.Valid))
            {
                _Logger.LogWarning($"One or more keys not valid: {args.BucketId}");
                return false;
            }

            return true;
        }
    }
}