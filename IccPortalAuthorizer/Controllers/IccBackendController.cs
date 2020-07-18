// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Controllers
{
    [Route("api")]
    public class IccBackendController : Controller
    {
        [HttpGet, Authorize(AuthenticationSchemes = "jwt")]
        public IActionResult Index()
        {
            return new JsonResult(new {ok = false, message = "not implemented yet", user = User.Identity.Name});
        }
        
        
        // TODO: Write Proxy routes
        
        
        
    }
}