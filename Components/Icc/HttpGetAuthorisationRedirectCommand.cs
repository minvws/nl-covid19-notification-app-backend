// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class HttpGetAuthorisationRedirectCommand
    {
        private readonly IIccPortalConfig _Configuration;
        private readonly ILogger<HttpGetAuthorisationRedirectCommand> _Logger;

        public HttpGetAuthorisationRedirectCommand(IIccPortalConfig configuration, ILogger<HttpGetAuthorisationRedirectCommand> logger)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Execute(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));
            _Logger.LogInformation("Executing Auth.Redirect on Host {CurrentHost}", httpContext.Request.Host.ToString());

            string authorizationCode = "test123";
            
            return new RedirectResult(_Configuration.FrontendBaseUrl + "/auth/callback?code=" + authorizationCode);
        }
    }
}