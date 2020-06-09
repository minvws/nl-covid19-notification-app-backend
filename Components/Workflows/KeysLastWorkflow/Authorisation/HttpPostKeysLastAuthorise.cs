// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.AuthorisationTokens;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation
{
    public class HttpPostKeysLastAuthorise
    {
        private readonly IKeysLastAuthorisationTokenValidator _AuthorisationTokenValidator;
        private readonly IKeysLastAuthorisationWriter _AuthorisationWriter;

        public HttpPostKeysLastAuthorise(IKeysLastAuthorisationTokenValidator authorisationTokenValidator, IKeysLastAuthorisationWriter authorisationWriter)
        {
            _AuthorisationTokenValidator = authorisationTokenValidator;
            _AuthorisationWriter = authorisationWriter;
        }

        public async Task<IActionResult> Execute(KeysLastAuthorisationArgs args)
        {
            //TODO validate args as a whole...
            if (!_AuthorisationTokenValidator.Valid(args.UploadAuthorisationToken))
            {
                //TODO log bad token
                return new OkResult();
            }

            await _AuthorisationWriter.Execute(args.TestId, args.UploadAuthorisationToken);
            return new OkResult();
        }
    }
}