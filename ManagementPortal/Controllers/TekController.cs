// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;

namespace ManagementPortal
{
    public class TekController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}