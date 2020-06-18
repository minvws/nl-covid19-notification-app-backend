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

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class IccAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IIccService _IccService;
        private readonly IccBackendContentDbContext _DbContext;
        private readonly ILogger<IccAuthenticationHandler> _logger;

        public IccAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IIccService IccService,
            IccBackendContentDbContext dbContext,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
            _DbContext = dbContext;
            _IccService = IccService;
            _logger = logger.CreateLogger<IccAuthenticationHandler>();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
            InfectionConfirmationCodeEntity Icc;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                string headerIccode = authHeader.ToString();
                Icc = await _IccService.Validate(headerIccode);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Icc");
            }

            if (Icc == null) return AuthenticateResult.Fail("Invalid Icc");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, Icc.Code)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}