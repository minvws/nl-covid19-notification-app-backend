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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.Api.IccBackend.Controllers
{
    public class WorkflowController : Controller
    {
        private readonly ILogger _Logger;

        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<ActionResult<AuthorisationResponse>> PostAuthorise([FromBody] AuthorisationArgs args, [FromServices] HttpPostAuthoriseLabConfirmationIdCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.WriteLabStart();

            var result = await command.ExecuteAsync(args);
            if(result == null)
            {
                return new BadRequestResult();
            }

            return Ok();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
