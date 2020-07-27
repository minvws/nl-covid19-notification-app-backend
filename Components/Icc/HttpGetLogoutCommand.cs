using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
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