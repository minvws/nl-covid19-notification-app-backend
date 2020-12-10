// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.IccPortal.Components;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    public class AccountController : Controller
    {
        [AllowAnonymous, HttpGet]
        public IActionResult AccessDenied([FromServices] HttpGetLogoutCommand logoutCommand, [FromServices] HttpGetAccessDeniedCommand accessDeniedCommand)
        {
            logoutCommand.ExecuteAsync(HttpContext); // logs out without using the redirectresult 
            return accessDeniedCommand.Execute(HttpContext);
        }
    }
}