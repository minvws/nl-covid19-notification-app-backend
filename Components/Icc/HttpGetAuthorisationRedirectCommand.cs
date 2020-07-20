using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    public class HttpGetAuthorisationRedirectCommand
    {
        private readonly IConfiguration _Configuration;
        private readonly JwtService _JwtService;

        public HttpGetAuthorisationRedirectCommand(IConfiguration configuration, JwtService jwtService)
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
            return new RedirectResult(_Configuration.GetValue("IccPortal:FrontendHost", "TODO Sensible default!!!!") + "/auth?token=" + jwtToken);
        }
    }
}