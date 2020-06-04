// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Arguments;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers.DevOps
{
    [ApiController]
    [Route("[controller]")]
    public class DevOpsController : ControllerBase
    {
        /// <summary>
        /// Provision Database.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/devops/ProvisionDb")]
        public async Task<IActionResult> ProvisionDb([FromServices]HttpPostProvisionDbCommand command)
        {
            return await command.Execute();
        }

        /// <summary>
        /// Generate new WorkFlows.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/devops/GenWorkFlows")]
        public IActionResult WorkFlows([FromBody]HttpPostGenerateWorkflowArguments arguments, [FromServices]HttpPostGenerateWorkflowCommand command)
        {
            return command.Execute(arguments);
        }

        /// <summary>
        /// Generate new ExposureKeySets.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/devops/GenExposureKeySets")]
        public async Task<IActionResult> ExposureKeySets([FromServices]HttpPostGenerateExposureKeySetsCommand command)
        {
            return await command.Execute();
        }

        /// <summary>
        /// Generate new authorizations.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/devops/GenAuthorize")]
        public async Task<IActionResult> Authorize([FromBody]HttpPostGenerateAuthorizeArguments arguments, [FromServices]HttpPostGenerateAuthorizeCommand command)
        {
            return await command.Execute(arguments);
        }
    }
}
