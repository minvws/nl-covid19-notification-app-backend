// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using System;
using System.Linq;

namespace ManagementPortal
{
    public class DataController : Controller
    {
        private readonly WorkflowDbContext _WorkflowDbContext;

        public DataController(WorkflowDbContext workflowDbContext)
        {
            if (workflowDbContext == null) throw new ArgumentNullException(nameof(workflowDbContext));

            _WorkflowDbContext = workflowDbContext;
        }

        // GET
        public IActionResult Index()
        {
            // _Logger.LogInformation("POST labverify triggered.");
            ViewData["teks"] = _WorkflowDbContext.TemporaryExposureKeys.Include(x => x.Owner).ToList();
            ViewData["wf"] = _WorkflowDbContext.KeyReleaseWorkflowStates.ToList();
            return View();
        }
    }
}