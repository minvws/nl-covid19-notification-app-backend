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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public class IccAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IIccService _IccService;
        private readonly IccBackendContentDbContext _DbContext;
        private readonly ILogger<IccAuthenticationHandler> _Logger;

        public IccAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IIccService iccService,
            IccBackendContentDbContext dbContext,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _DbContext = dbContext;
            _IccService = iccService;
            _Logger = logger.CreateLogger<IccAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Missing Authorization Header");
            }
                
            InfectionConfirmationCodeEntity infectionConfirmationCodeEntity;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var authHeaderString = authHeader.ToString();
                infectionConfirmationCodeEntity = await _IccService.Validate(authHeaderString);
            }
            catch(Exception e)
            {    
                _Logger.LogCritical(e.ToString());
                return AuthenticateResult.Fail("Invalid Icc");
            }

            if (infectionConfirmationCodeEntity == null) return AuthenticateResult.Fail("Invalid Icc");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, infectionConfirmationCodeEntity.Code)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}