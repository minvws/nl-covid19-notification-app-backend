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
        private readonly IIccPortalConfig _configuration;
        private readonly ILogger _logger;
        private readonly IAuthCodeService _authCodeService;
        private readonly IJwtService _jwtService;
        private readonly ITheIdentityHubService _theIdentityHubService;
        private readonly HttpGetLogoutCommand _logoutCommand;

        public HttpGetAuthorisationRedirectCommand(IIccPortalConfig configuration,
            ILogger<HttpGetAuthorisationRedirectCommand> logger, IAuthCodeService authCodeService,
            IJwtService jwtService, ITheIdentityHubService theIdentityHubService, HttpGetLogoutCommand logoutCommand)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authCodeService = authCodeService ?? throw new ArgumentNullException(nameof(authCodeService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _theIdentityHubService = theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
            _logoutCommand = logoutCommand ?? throw new ArgumentNullException(nameof(logoutCommand));
        }

        public async Task<IActionResult> ExecuteAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            _logger.LogInformation("Executing Auth.Redirect on Host {CurrentHost}.",
                httpContext.Request.Host.ToString());

            // check httpContext claims on AccessToken validity
            if (!await _theIdentityHubService.VerifyClaimTokenAsync(httpContext.User.Claims))
            {
                await _logoutCommand.ExecuteAsync(httpContext);
                return new RedirectResult(httpContext.Request.Path); // redirect to {prefix}/Auth/Redirect to trigger login
            }

            var authorizationCode = await _authCodeService.GenerateAuthCodeAsync(httpContext.User);

            return new RedirectResult(_configuration.FrontendBaseUrl + "/auth/callback?code=" + authorizationCode);
        }
    }
}
