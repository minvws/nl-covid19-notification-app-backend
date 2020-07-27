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
        private readonly LabVerifyArgsValidator _LabVerifyArgsValidator;

        public HttpPostLabVerifyCommand(LabVerificationAuthorisationCommand labVerificationAuthorisationCommand, LabVerifyArgsValidator labVerifyArgsValidator)
        {
            _LabVerificationAuthorisationCommand = labVerificationAuthorisationCommand;
            _LabVerifyArgsValidator = labVerifyArgsValidator;
        }

        public async Task<IActionResult> Execute(LabVerifyArgs args)
        {
            if (!_LabVerifyArgsValidator.Validate(args))
                return new BadRequestResult();

            var result = await _LabVerificationAuthorisationCommand.Execute(args);

            return new OkObjectResult(result);
        }
    }
}