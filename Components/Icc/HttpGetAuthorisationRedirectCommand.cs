using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class HttpGetAuthorisationRedirectCommand
    {
        private readonly IIccPortalConfig _Configuration;
        private readonly IJwtService _JwtService;

        public HttpGetAuthorisationRedirectCommand(IIccPortalConfig configuration, IJwtService jwtService)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public IActionResult Execute(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));

            var jwtToken = _JwtService.Generate(httpContext.User);

            // temporary claim payload redirect solution for demo purposes
            return new RedirectResult(_Configuration.FrontendBaseUrl + "/auth?token=" + jwtToken);
        }
    }
}