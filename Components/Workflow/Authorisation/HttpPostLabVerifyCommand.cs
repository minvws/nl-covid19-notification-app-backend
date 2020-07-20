// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using JWT.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostLabVerifyCommand
    {
        private readonly LabVerificationAuthorisationCommand _LabVerificationAuthorisationCommand;

        public HttpPostLabVerifyCommand(LabVerificationAuthorisationCommand labVerificationAuthorisationCommand)
        {
            _LabVerificationAuthorisationCommand = labVerificationAuthorisationCommand;
        }

        public async Task<IActionResult> Execute(LabVerifyArgs args)
        {
            if (_LabVerificationAuthorisationCommand.Validate(args))
                return new BadRequestResult();

            var result = await _LabVerificationAuthorisationCommand.Execute(args);

            return new OkObjectResult(result);
        }
    }
}