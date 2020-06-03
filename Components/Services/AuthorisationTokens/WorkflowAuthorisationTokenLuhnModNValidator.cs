// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens
{
    public class WorkflowAuthorisationTokenLuhnModNValidator : IWorkflowAuthorisationTokenValidator
    {
        private readonly LuhnModNValidator _Validator;

        public WorkflowAuthorisationTokenLuhnModNValidator(ILuhnModNConfig config)
        {
            _Validator = new LuhnModNValidator(config);
        }

        public bool IsValid(string value) => _Validator.Validate(value);
    }
}