// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace ManagementPortal.Controllers
{
    public class ContentController : Controller
    {
        // GET
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost, Route("content")]
        public IActionResult PostContent([FromForm] ContentArgs contentArgs)
        {
            return new OkObjectResult(contentArgs);
        }
    }
}