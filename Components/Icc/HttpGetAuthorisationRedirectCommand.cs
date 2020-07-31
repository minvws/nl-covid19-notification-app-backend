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
        private readonly IJwtService _JwtService;
        private readonly ILogger<HttpGetAuthorisationRedirectCommand> _Logger;

        public HttpGetAuthorisationRedirectCommand(IIccPortalConfig configuration, IJwtService jwtService, ILogger<HttpGetAuthorisationRedirectCommand> logger)
        {
            _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IActionResult Execute(HttpContext httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException(nameof(httpContext));
                    
            _Logger.LogInformation($"Executing Auth.Redirect on Host {httpContext.Request.Host.ToString()}");
            var jwtToken = _JwtService.Generate(httpContext.User);

            // temporary claim payload redirect solution for demo purposes
            return new RedirectResult(_Configuration.FrontendBaseUrl + "/auth?token=" + jwtToken);
        }
    }
}