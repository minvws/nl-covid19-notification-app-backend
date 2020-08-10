// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.DecoyKeys;
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

        [ResponsePaddingFilterFactory]
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.ReleaseTeks)]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostReleaseTeksCommand2 command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.LogInformation("POST postkeys triggered.");
            return await command.Execute(sig, Request);
        }

        [ResponsePaddingFilterFactory]
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.Register)]
        public async Task<IActionResult> PostSecret([FromServices]HttpPostRegisterSecret command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.LogInformation("POST register triggered.");
            return await command.Execute();
        }

        [ResponsePaddingFilterFactory]
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.RandomNoise)]
        public async Task<IActionResult> StopKeys([FromServices] HttpPostDecoyKeysCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.LogInformation("POST stopkeys triggered.");
            return await command.Execute();
        }

        [HttpGet]
        [Route("/")]
        public IActionResult AssemblyDump() => new DumpAssembliesToPlainText().Execute();
    }
}
