// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Diagnostics;
using IccPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _Logger;
        private FrontendService _FrontendService;

        public HomeController(ILogger<HomeController> logger, FrontendService frontendService)
        {
            _Logger = logger;
            _FrontendService = frontendService;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return Redirect(_FrontendService.GetFrontendLoginUrl("/"));
        }
            
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return new JsonResult(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}