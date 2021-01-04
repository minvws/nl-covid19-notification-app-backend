// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands
{
    public class HttpGetAuthorisationRedirectCommand
    {
        private readonly IIccPortalConfig _Configuration;
        private readonly ILogger<HttpGetAuthorisationRedirectCommand> _Logger;
        private readonly IAuthCodeService _AuthCodeService;
        private readonly IJwtService _JwtService;
        private readonly ITheIdentityHubService _TheIdentityHubService;
        private readonly HttpGetLogoutCommand _LogoutCommand;

        public HttpGetAuthorisationRedirectCommand(IIccPortalConfig configuration,
            ILogger<HttpGetAuthorisationRedirectCommand> logger, IAuthCodeService authCodeService,
            IJwtService jwtService, ITheIdentityHubService theIdentityHubService, HttpGetLogoutCommand logoutCommand)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _AuthCodeService = authCodeService ?? throw new ArgumentNullException(nameof(authCodeService));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _TheIdentityHubService = theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
            _LogoutCommand = logoutCommand ?? throw new ArgumentNullException(nameof(logoutCommand));
        }

        public async Task<IActionResult> ExecuteAsync(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            _Logger.WriteRedirecting(httpContext.Request.Host.ToString());

            // check httpContext claims on AccessToken validity
            if (!await _TheIdentityHubService.VerifyClaimTokenAsync(httpContext.User.Claims))
            {
                await _LogoutCommand.ExecuteAsync(httpContext);
                return new RedirectResult(httpContext.Request.Path); // redirect to {prefix}/Auth/Redirect to trigger login
            }

            var authorizationCode = await _AuthCodeService.GenerateAuthCodeAsync(httpContext.User);

            return new RedirectResult(_Configuration.FrontendBaseUrl + "/auth/callback?code=" + authorizationCode);
        }
    }
}