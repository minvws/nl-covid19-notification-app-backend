// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {

        private readonly ILogger _Logger;

        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [HttpPost]
        [Route(EndPointNames.MobileAppApi.ReleaseTeks)]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostReleaseTeksCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));

            _Logger.LogInformation("POST postkeys triggered.");
            return await command.Execute(sig, Request);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.Register)]
        public async Task<IActionResult> PostSecret([FromServices]HttpPostRegisterSecret command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));

            _Logger.LogInformation("POST register triggered.");
            return await command.Execute();
        }

        [HttpPost, Authorize(AuthenticationSchemes = "icc_jwt")]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody] AuthorisationArgs args, [FromServices] HttpPostAuthorise command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));

            _Logger.LogInformation("POST lab confirmation triggered.");
            return await command.Execute(args);
        }
        
        [HttpPost, Authorize(AuthenticationSchemes = "icc_jwt")]
        [Route(EndPointNames.CaregiversPortalApi.LabVerify)]
        public async Task<IActionResult> PostKeysAreUploaded([FromBody] LabVerifyArgs args, [FromServices] HttpPostLabVerify command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));

            _Logger.LogInformation("POST labverify triggered.");
            return await command.Execute(args);
        }
    }
}
