// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation
{
    public class HttpPostWorkflowAuthoriseCommand
    {
        private readonly IWorkflowAuthorisationTokenValidator _WorkflowAuthorisationTokenValidator;
        private readonly IWorkflowAuthorisationWriter _WorkflowAuthorisationWriter;
        public HttpPostWorkflowAuthoriseCommand(IWorkflowAuthorisationWriter WorkflowAuthorisationWriter, IWorkflowAuthorisationTokenValidator WorkflowAuthorisationTokenValidator)
        {
            _WorkflowAuthorisationWriter = WorkflowAuthorisationWriter;
            _WorkflowAuthorisationTokenValidator = WorkflowAuthorisationTokenValidator;
        }

        public IActionResult Execute(WorkflowAuthorisationArgs args)
        {
            if (!ValidateAndCleanRequestMessage(args))
                return new BadRequestResult();

            _WorkflowAuthorisationWriter.Execute(args);
            return new OkResult();
        }

        private bool ValidateAndCleanRequestMessage(WorkflowAuthorisationArgs args)
        {
            if (args == null)
                return false;

            return _WorkflowAuthorisationTokenValidator.IsValid(args.Token);
        }
    }
}
