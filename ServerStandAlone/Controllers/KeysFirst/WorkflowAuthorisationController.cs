// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.KeysFirstWorkflow.WorkflowAuthorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers.KeysFirst
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowAuthorisationController : ControllerBase
    {
        /// <summary>
        /// A Workflow is Activated.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/keysfirst/authorise")]
        public IActionResult PostActivateTemporaryExposureKey([FromBody]WorkflowAuthorisationArgs args, [FromServices]HttpPostWorkflowAuthoriseCommand command)
        {
            return command.Execute(args);
        }
    }
}
