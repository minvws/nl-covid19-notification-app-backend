// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Services;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Services;
using System;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private readonly FrontendService _FrontendService;
        private readonly JwtService _JwtService;

        public AuthController(FrontendService frontendService, JwtService jwtService)
        {
            _FrontendService = frontendService ?? throw new ArgumentNullException(nameof(frontendService));
            _JwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        }

        public IActionResult Logout()
        {
            Response.Cookies.Delete(".AspNetCore.Cookies");

            return Redirect(_FrontendService.GetFrontendLoginUrl("/"));
        }
        public IActionResult Redirect()
        {
            var jwtToken = _JwtService.GenerateJwt(User);

            // temporary claim payload redirect solution for demo purposes
            return Redirect(_FrontendService.RedirectSuccessfulLogin(jwtToken));
        }
    }
}