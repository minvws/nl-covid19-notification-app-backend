// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IccPortal.Controllers
{
    public class AuthController : Controller
    {
        // GET
        [Authorize]
        public IActionResult Index()
        {
            // TODO: Check Role Claims and redirect to correct page. 
            
            
            return new JsonResult(User.Claims);
        }
    }
}