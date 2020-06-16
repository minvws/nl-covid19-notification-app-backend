// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC
{
    public class ICCAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IICCService _ICCService;
        private readonly ICCBackendContentDbContext _DbContext;
        private readonly ILogger<ICCAuthenticationHandler> _logger;

        public ICCAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IICCService ICCService,
            ICCBackendContentDbContext dbContext,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _DbContext = dbContext;
            _ICCService = ICCService;
            _logger = logger.CreateLogger<ICCAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
            InfectionConfirmationCodeEntity ICC;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                string headerICCode = authHeader.ToString();
                ICC = await _ICCService.Validate(headerICCode);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid ICC");
            }

            if (ICC == null) return AuthenticateResult.Fail("Invalid ICC");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, ICC.Code)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}