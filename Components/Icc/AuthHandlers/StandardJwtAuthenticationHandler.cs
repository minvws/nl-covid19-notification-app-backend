// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class StandardJwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string AccessTokenElement = "access_token";

        private readonly IJwtService _JwtService;
        private readonly IJwtValidatorService _JwtValidatorService;

        public StandardJwtAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtService jwtService, IJwtValidatorService jwtValidatorService) : base(options, loggerFactory, encoder, clock)
        {
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _JwtValidatorService = jwtValidatorService ?? throw new ArgumentNullException(nameof(jwtValidatorService));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var headerValue))
            {
                Logger.LogInformation("Missing authorization header.");
                return AuthenticateResult.Fail("Missing authorization header.");
            }

            if (!AuthenticationHeaderValue.TryParse(headerValue, out var authHeader))
            {
                Logger.LogInformation("Invalid authorization header.");
                return AuthenticateResult.Fail("Invalid authorization header.");
            }

            var jwt = authHeader.ToString().Replace("Bearer ", "").Trim();
            // AccessTokenElement
            if (!_JwtValidatorService.IsValid(jwt))
            {
                Logger.LogWarning($"Invalid jwt token - {jwt}.");
                return AuthenticateResult.Fail("Invalid jwt token.");
            }
            var decodedClaims = _JwtService.Decode(jwt);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, decodedClaims["id"]),
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}