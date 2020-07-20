// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [Authorize(AuthenticationSchemes = "jwt")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("AuthenticatedUser")]
        public IActionResult AuthenticatedUser([FromServices] HttpGetUserClaimCommand command)
            => command.Execute(HttpContext);
    }
}