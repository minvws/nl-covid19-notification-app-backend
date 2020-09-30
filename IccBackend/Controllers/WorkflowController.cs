// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Auth.Handlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Controllers
{
    [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
    public class WorkflowController : Controller
    {
        private readonly ILogger _Logger;

        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody] AuthorisationArgs args, [FromServices] HttpPostAuthoriseCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.LogInformation("POST lab confirmation triggered.");
            return await command.Execute(args);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
