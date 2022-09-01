// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Code;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands
{
    public class HttpPostAuthorizationTokenCommand
    {
        private readonly ILogger<HttpPostAuthorizationTokenCommand> _logger;
        private readonly IJwtService _jwtService;
        private readonly IAuthCodeService _authCodeService;

        public HttpPostAuthorizationTokenCommand(ILogger<HttpPostAuthorizationTokenCommand> logger,
            IJwtService jwtService, IAuthCodeService authCodeService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _authCodeService = authCodeService ?? throw new ArgumentNullException(nameof(authCodeService));
        }

        public IActionResult Execute(HttpContext httpContext, TokenAuthorisationArgs args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            var claims = _authCodeService.GetClaimsByAuthCode(args.Code);
            if (claims == null)
            {
                return new UnauthorizedResult();
            }

            _authCodeService.RevokeAuthCode(args.Code);

            var jwtToken = _jwtService.Generate(claims);
            return new OkObjectResult(new
            {
                Token = jwtToken
            });
        }
    }
}
