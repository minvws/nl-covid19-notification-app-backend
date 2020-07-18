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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.AuthHandlers
{
    public class JwtAuthorizationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly JwtService _JwtService;

        public JwtAuthorizationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory,
            UrlEncoder encoder,
            ISystemClock clock,
            JwtService jwtService) : base(options, loggerFactory, encoder, clock)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Logger.LogInformation($"Missing authorization header.");
                return AuthenticateResult.Fail("Missing authorization header.");
            }

            bool isValidJwt;
            string jwtToken;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]); //TODO if this was the exception, use the try/find pattern
                var authHeaderString = authHeader.ToString();
                jwtToken = authHeaderString.Replace("Bearer ", "").Trim();
                isValidJwt = _JwtService.IsValidJwt(jwtToken);
            }
            catch (Exception e) //TODO shouldnt need this at all.
            {
                Logger.LogError(e.ToString());
                return AuthenticateResult.Fail("Error invalid jwt.");
            }

            if (!isValidJwt)
            {
                Logger.LogWarning($"Invalid jwt - {jwtToken}.");
                return AuthenticateResult.Fail("Invalid jwt.");
            }

            // TODO: Add other payload items as well to current claims

            var jwtPayload = _JwtService.DecodeJwt(jwtToken);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, jwtPayload["name"].ToString()),
                new Claim(ClaimTypes.NameIdentifier, jwtPayload["id"].ToString()),
                new Claim(ClaimTypes.Email, jwtPayload["email"].ToString())
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}