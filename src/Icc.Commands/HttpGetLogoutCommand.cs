// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands
{
    public class HttpGetLogoutCommand
    {
        private readonly IIccPortalConfig _Configuration;
        private readonly ITheIdentityHubService _TheIdentityHubService;

        public HttpGetLogoutCommand(IIccPortalConfig configuration, ITheIdentityHubService theIdentityHubService)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _TheIdentityHubService =
                theIdentityHubService ?? throw new ArgumentNullException(nameof(theIdentityHubService));
        }

        public async Task<IActionResult> ExecuteAsync(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var accessToken = httpContext.User?.Claims
                .FirstOrDefault((c => c.Type == TheIdentityHubClaimTypes.AccessToken))
                ?.Value;
            if (accessToken != null)
                await _TheIdentityHubService.RevokeAccessTokenAsync(accessToken);

            httpContext.Response.Cookies.Delete(".AspNetCore.Cookies");
            await httpContext.SignOutAsync();

            return new RedirectResult(_Configuration.FrontendBaseUrl);
        }
    }
}