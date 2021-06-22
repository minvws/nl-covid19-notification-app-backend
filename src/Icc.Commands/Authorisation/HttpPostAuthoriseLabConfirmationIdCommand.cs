// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class HttpPostAuthoriseLabConfirmationIdCommand
    {
        private readonly AuthorizeLabConfirmationIdCommand _authorisationWriter;
        private readonly AuthorisationArgsValidator _authorisationArgsValidator;
        private readonly ILogger _logger;

        public HttpPostAuthoriseLabConfirmationIdCommand(AuthorizeLabConfirmationIdCommand authorisationWriter, AuthorisationArgsValidator authorisationArgsValidator, ILogger<HttpPostAuthoriseLabConfirmationIdCommand> logger)
        {
            _authorisationWriter = authorisationWriter ?? throw new ArgumentNullException(nameof(authorisationWriter));
            _authorisationArgsValidator = authorisationArgsValidator ?? throw new ArgumentNullException(nameof(authorisationArgsValidator));
            _logger = logger;
        }

        public async Task<AuthorisationResponse> ExecuteAsync(AuthorisationArgs args)
        {
            if (_logger.LogValidationMessages(_authorisationArgsValidator.Validate(args)))
            {
                return null;
            }

            var success = await _authorisationWriter.ExecuteAsync(args);

            var response = new AuthorisationResponse
            {
                Valid = success,
            };

            return response;
        }
    }
}
