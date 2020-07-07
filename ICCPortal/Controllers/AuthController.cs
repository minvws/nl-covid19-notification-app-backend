// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Controllers
{
    [Authorize]
    public class AuthController : Controller
    {
        private FrontendService _FrontendService;
        private JwtService _JwtService;

        private static readonly List<string> ClaimTypeBlackList = new List<string>()
        {
            "http://schemas.u2uconsult.com/ws/2014/03/identity/claims/accesstoken",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
            "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"
        };

        public AuthController(FrontendService frontendService, JwtService jwtService)
        {
            _FrontendService = frontendService;
            _JwtService = jwtService;
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
            return Redirect(_FrontendService.RedirectSuccesfullLogin(jwtToken));
        }

        private Dictionary<string, string> GetClaims()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            User.Claims.Where(c => !ClaimTypeBlackList.Contains(c.Type)).ToList()
                .ForEach((c) => { result.Add(c.Type, c.Value); });
            return result;
        }

        public IActionResult Introspection()
        {
            return new JsonResult(GetClaims());
        }
    }
}