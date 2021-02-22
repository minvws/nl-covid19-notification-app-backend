// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Handlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Models;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.Icc.WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _Logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Policy = "TelefonistRole", AuthenticationSchemes = TheIdentityHubDefaults.AuthenticationScheme)]
        [HttpGet]
        public Task<IActionResult> Redirect([FromServices] HttpGetAuthorisationRedirectCommand command) =>  command.ExecuteAsync(HttpContext);


        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Token([FromBody] TokenAuthorisationArgs args, [FromServices] HttpPostAuthorizationTokenCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.WriteAuthStart();
            return await command.ExecuteAsync(HttpContext, args);
        }

        [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
        [HttpGet]
        public IActionResult User([FromServices] HttpGetUserClaimCommand command) => command.Execute(HttpContext);

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Logout([FromServices] HttpGetLogoutCommand command) => await command.ExecuteAsync(HttpContext);
    }
}
