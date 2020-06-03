// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows
{
    public class WorkflowValidator : IWorkflowValidator
    {
        private readonly IWorkflowValidatorConfig _Config;
        private readonly IWorkflowAuthorisationTokenValidator _AuthorisationTokenValidator;
        private readonly ITemporaryExposureKeyValidator _TemporaryExposureKeyValidator;

        public WorkflowValidator(IWorkflowValidatorConfig config, IWorkflowAuthorisationTokenValidator authorisationTokenValidator, ITemporaryExposureKeyValidator temporaryExposureKeyValidator)
        {
            _Config = config;
            _AuthorisationTokenValidator = authorisationTokenValidator;
            _TemporaryExposureKeyValidator = temporaryExposureKeyValidator;
        }

        public bool Validate(WorkflowArgs args)
        {
            if (args == null)
                return false;

            if (!_AuthorisationTokenValidator.IsValid(args.Token))
                return false;

            if (_Config.WorkflowKeyCountMin > args.Items.Length
                || args.Items.Length > _Config.WorkflowKeyCountMax)
                return false;

            return args.Items.All(_TemporaryExposureKeyValidator.Valid);
        }
    }
}