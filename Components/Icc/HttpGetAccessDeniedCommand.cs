// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class HttpGetAccessDeniedCommand
    {
        private readonly IIccPortalConfig _Configuration;
        private readonly ILogger<HttpGetAccessDeniedCommand> _Logger;

        public HttpGetAccessDeniedCommand(IIccPortalConfig configuration,
            ILogger<HttpGetAccessDeniedCommand> logger)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Execute(HttpContext httpContext)
        {
            _Logger.LogInformation("AccessDenied for login, insufficient role");
            var redirectUrl = _Configuration.FrontendBaseUrl + "/?e=access_denied";
            return new RedirectResult(redirectUrl);
        }
    }
}