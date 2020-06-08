// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation
{
    public class HttpPostKeysLastAuthorize
    {
        private readonly IKeysLastAuthorisationTokenValidator _AuthorisationTokenValidator;
        private readonly IKeysLastAuthorisationWriter _AuthorisationWriter;

        public HttpPostKeysLastAuthorize(IKeysLastAuthorisationTokenValidator authorisationTokenValidator, IKeysLastAuthorisationWriter authorisationWriter)
        {
            _AuthorisationTokenValidator = authorisationTokenValidator;
            _AuthorisationWriter = authorisationWriter;
        }

        public IActionResult Execute(KeysLastAuthorisationArgs args)
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