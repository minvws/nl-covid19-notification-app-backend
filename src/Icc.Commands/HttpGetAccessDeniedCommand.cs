// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands
{
    public class HttpGetAccessDeniedCommand
    {
        private readonly IIccPortalConfig _configuration;
        private readonly ILogger _logger;

        public HttpGetAccessDeniedCommand(IIccPortalConfig configuration,
            ILogger<HttpGetAccessDeniedCommand> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Execute(HttpContext httpContext)
        {
            _logger.LogInformation("AccessDenied for login, insufficient role.");
            var redirectUrl = _configuration.FrontendBaseUrl + "/?e=access_denied";
            return new RedirectResult(redirectUrl);
        }
    }
}
