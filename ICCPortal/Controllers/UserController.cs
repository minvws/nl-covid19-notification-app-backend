// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                user=User.Identity.Name
            });
        }
        
    }
}