// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using System;
using System.Threading.Tasks;

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

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.RandomNoise)]
        public async Task<IActionResult> StopKeys([FromQuery] byte[] sig, [FromServices] HttpPostDecoyKeysCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (_Logger == null) throw new ArgumentNullException(nameof(_Logger));

            _Logger.LogInformation("POST stopkeys  triggered.");
            return await command.Execute();
        }
    }
}
