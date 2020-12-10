// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class HttpPostAuthoriseCommand
    {
        private readonly AuthorisationWriterCommand _AuthorisationWriter;
        private readonly AuthorisationArgsValidator _AuthorisationArgsValidator;
        private readonly ILogger _Logger;

        public HttpPostAuthoriseCommand(AuthorisationWriterCommand authorisationWriter,AuthorisationArgsValidator authorisationArgsValidator, ILogger<HttpPostAuthoriseCommand> logger)
        {
            _AuthorisationWriter = authorisationWriter ?? throw new ArgumentNullException(nameof(authorisationWriter));
            _AuthorisationArgsValidator = authorisationArgsValidator ?? throw new ArgumentNullException(nameof(authorisationArgsValidator));
            _Logger = logger;
        }

        public async Task<IActionResult> ExecuteAsync(AuthorisationArgs args)
        {
            if(_Logger.LogValidationMessages( _AuthorisationArgsValidator.Validate(args)))
                return new BadRequestResult();

            var newPollToken = await _AuthorisationWriter.ExecuteAsync(args);

            var response = new AuthorisationResponse
            {
                Valid = newPollToken != null,
                PollToken = newPollToken
            };

            return new OkObjectResult(response);
        }
    }
}