// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class HttpGetLogoutCommand
    {
        private readonly IIccPortalConfig _Configuration;

        public HttpGetLogoutCommand(IIccPortalConfig configuration)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IActionResult Execute(HttpContext httpContext)
        {
            if (httpContext == null) 
                throw new ArgumentNullException(nameof(httpContext));

            httpContext.Response.Cookies.Delete(".AspNetCore.Cookies");
            return new RedirectResult(_Configuration.FrontendBaseUrl);
        }
    }
}