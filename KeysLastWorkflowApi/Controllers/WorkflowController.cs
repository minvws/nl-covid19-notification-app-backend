// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using Serilog;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.WorkflowApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.ReleaseTeks)]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostReleaseTeksCommand command, [FromServices]ILogger logger)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Information("POST postkeys triggered.");
            return await command.Execute(sig, Request);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.RegisterSecret)]
        public async Task<IActionResult> PostSecret([FromServices]HttpPostRegisterSecret command, [FromServices] ILogger logger)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Information("POST register triggered.");
            return await command.Execute();
        }

        [HttpPost, Authorize(AuthenticationSchemes = "icc_jwt")]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody] AuthorisationArgs args, [FromServices] HttpPostAuthorise command, [FromServices] ILogger logger)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.Information("POST lab confirmation triggered.");
            return await command.Execute(args);
        }
    }
}
