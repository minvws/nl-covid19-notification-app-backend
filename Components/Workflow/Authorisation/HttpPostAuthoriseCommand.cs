// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostAuthoriseCommand
    {
        private readonly AuthorisationWriterCommand _AuthorisationWriter;
        private readonly AuthorisationArgsValidator _AuthorisationArgsValidator;

        public HttpPostAuthoriseCommand(AuthorisationWriterCommand authorisationWriter,AuthorisationArgsValidator authorisationArgsValidator)
        {
            _AuthorisationWriter = authorisationWriter ?? throw new ArgumentNullException(nameof(authorisationWriter));
            _AuthorisationArgsValidator = authorisationArgsValidator ?? throw new ArgumentNullException(nameof(authorisationArgsValidator));
        }

        public async Task<IActionResult> Execute(AuthorisationArgs args)
        {
            if (!_AuthorisationArgsValidator.Validate(args))
                return new BadRequestResult();

            var result = await _AuthorisationWriter.Execute(args);

            return new OkObjectResult(result);
        }
    }
}