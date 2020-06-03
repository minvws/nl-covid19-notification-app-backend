// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow
{
    public class HttpPostTokenFirstAuthorize
    {
        private readonly ITokenFirstAuthorisationTokenValidator _AuthorisationTokenValidator;
        private readonly ITokenFirstAuthorisationWriter _AuthorisationWriter;

        public HttpPostTokenFirstAuthorize(ITokenFirstAuthorisationTokenValidator authorisationTokenValidator, ITokenFirstAuthorisationWriter authorisationWriter)
        {
            _AuthorisationTokenValidator = authorisationTokenValidator;
            _AuthorisationWriter = authorisationWriter;
        }

        public IActionResult Execute(TokenFirstAuthorisationArgs args)
        {
            if (!_AuthorisationTokenValidator.Valid(args))
            {
                //TODO log bad token
                return new OkResult();
            }

            _AuthorisationWriter.Execute(args.Token);
            return new OkResult();
        }
    }
}