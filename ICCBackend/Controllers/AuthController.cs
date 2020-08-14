// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using TheIdentityHub.AspNetCore.Authentication;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [Authorize(Policy = "TelefonistRole")]
    public class AuthController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Logout([FromServices] HttpGetLogoutCommand command)
            => await command.Execute(HttpContext);

        [HttpGet]
        public IActionResult Redirect([FromServices] HttpGetAuthorisationRedirectCommand command)
            => command.Execute(HttpContext);

    }
}