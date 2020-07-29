// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ManagementPortal;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace ManagementPortal
{
    public class DataController : Controller
    {
        // GET
        public async Task<IActionResult> Index([FromServices] HttpGetTEKsCommand httpGetTeKsCommand,
            [FromServices] HttpGetWorkflowStatesCommand httpGetWorkflowStatesCommand)
        {
            if (httpGetTeKsCommand == null) throw new ArgumentNullException(nameof(httpGetTeKsCommand));
            if (httpGetWorkflowStatesCommand == null)
                throw new ArgumentNullException(nameof(httpGetWorkflowStatesCommand));

            // _Logger.LogInformation("POST labverify triggered.");
            ViewData["teks"] = await httpGetTeKsCommand.Execute();
            ViewData["wf"] = await httpGetWorkflowStatesCommand.Execute();
            return View();
        }
    }
}