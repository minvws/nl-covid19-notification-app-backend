// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace IccPortal.Controllers
{
    public class IccBackendController : Controller
    {
        [HttpGet, Authorize(Policy = "TelefonistRole")]
        public IActionResult Index()
        {
            return new JsonResult(new {ok = false, message = "not implemented yet"});
        }
    }
}