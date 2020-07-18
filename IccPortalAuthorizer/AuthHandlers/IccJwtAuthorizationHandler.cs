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
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.AuthHandlers
{
    public class IccJwtAuthorizationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtService _JwtService;
        private readonly ILogger _Logger;

        public IccJwtAuthorizationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtService jwtService) : base(options, logger, encoder, clock)
        {
            _JwtService = jwtService;
            _Logger = logger.CreateLogger<ILogger<IccJwtAuthorizationHandler>>(); //TODO there was resolve warning here cos the name clashed with JwtAuthorizationHandler in lib namespace
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            bool isValidJwt;
            string jwtToken;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var authHeaderString = authHeader.ToString();
                jwtToken = authHeaderString.Replace("Bearer ", "").Trim();
                isValidJwt = _JwtService.IsValidJwt(jwtToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _Logger.LogCritical(e.ToString());
                return AuthenticateResult.Fail("Invalid Jwt");
            }

            if (!isValidJwt) return AuthenticateResult.Fail("Invalid Jwt");

            // TODO: Add other payload items aswell to current claims
            
            var jwtPayload = _JwtService.DecodeJwt(jwtToken);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, jwtPayload["name"].ToString())
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}