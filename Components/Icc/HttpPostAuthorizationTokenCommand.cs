// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostAuthorizationTokenCommand
    {
        private readonly ILogger<HttpPostAuthorizationTokenCommand> _Logger;
        private readonly IJwtService _JwtService;
        private readonly AuthCodeService _AuthCodeService;

        public HttpPostAuthorizationTokenCommand(ILogger<HttpPostAuthorizationTokenCommand> logger,
            IJwtService jwtService, AuthCodeService authCodeService)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _AuthCodeService = authCodeService ?? throw new ArgumentNullException(nameof(authCodeService));
        }

        public async Task<IActionResult> Execute(HttpContext httpContext, TokenAuthorisationArgs args)
        {
            if (!_AuthCodeService.TryGetClaims(args.Code, out ClaimsPrincipal claims))
            {
                return new UnauthorizedResult();
            }
            var jwtToken = _JwtService.Generate(claims);
            return new OkObjectResult(new
            {
                Token = jwtToken
            });
        }
    }

    public class TokenAuthorisationArgs
    {
        [Required] public string Code { get; set; }
    }
}