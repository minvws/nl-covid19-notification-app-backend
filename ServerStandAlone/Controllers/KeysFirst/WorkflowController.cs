// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers.KeysFirst
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        [HttpPost]
        [Route("keysfirst/Workflow")]
        public IActionResult PostWorkflow([FromBody]WorkflowArgs args, [FromServices]HttpPostWorkflowCommand command)
        {
            return command.Execute(args);
        }
    }
}