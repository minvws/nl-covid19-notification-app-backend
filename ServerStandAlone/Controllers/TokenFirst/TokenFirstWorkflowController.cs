// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.TokenFirstWorkflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers.TokenFirst
{
    [ApiController]
    [Route("[controller]")]
    public class TokenFirstWorkflowController : ControllerBase
    {
        [HttpPost]
        [Route("tokenfirst/Workflow")]
        public IActionResult PostWorkflow([FromBody]WorkflowArgs args, [FromServices]HttpPostTokenFirstWorkflowCommand command)
        {
            return command.Execute(args);
        }

        [HttpPost]
        [Route("tokenfirst/secret")]
        public IActionResult PostSecret([FromBody]TokenFirstSecretArgs args, [FromServices]HttpPostTokenFirstSecret command)
        {
            return command.Execute(args);
        }

        [HttpPost]
        [Route("tokenfirst/authorise")]
        public IActionResult PostAuthorise([FromBody]TokenFirstAuthorisationArgs args, [FromServices]HttpPostTokenFirstAuthorize command)
        {
            return command.Execute(args);
        }
    }
}
