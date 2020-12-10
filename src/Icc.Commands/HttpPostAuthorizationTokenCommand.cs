// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
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
        private readonly ILogger<HttpPostAuthorizationTokenCommand> _Logger;
        private readonly IJwtService _JwtService;
        private readonly IAuthCodeService _AuthCodeService;

        public HttpPostAuthorizationTokenCommand(ILogger<HttpPostAuthorizationTokenCommand> logger,
            IJwtService jwtService, IAuthCodeService authCodeService)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _AuthCodeService = authCodeService ?? throw new ArgumentNullException(nameof(authCodeService));
        }

        public async Task<IActionResult> ExecuteAsync(HttpContext httpContext, TokenAuthorisationArgs args)
        {
            if(args == null) throw new ArgumentNullException(nameof(args));

            var claims = await _AuthCodeService.GetClaimsByAuthCodeAsync(args.Code);
            if(claims == null)
            {
                return new UnauthorizedResult();
            }

            //TODO: add sliding expiry time to distributed cache
            await _AuthCodeService.RevokeAuthCodeAsync(args.Code);
            
            var jwtToken = _JwtService.Generate(claims);
            return new OkObjectResult(new
            {
                Token = jwtToken
            });
        }
    }
}