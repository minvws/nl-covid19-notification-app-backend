// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Controllers
{
    [Authorize(AuthenticationSchemes = "jwt")]
    public class UserController : Controller
    {
        [HttpGet, Route("user/@me")]
        public IActionResult Me()
        {
            return new JsonResult(new
            {
                user = new
                {
                    id = User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.NameIdentifier))?.Value
                }
            });
        }
    }
}