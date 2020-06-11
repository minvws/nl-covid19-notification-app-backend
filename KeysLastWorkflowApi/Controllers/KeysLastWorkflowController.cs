// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.KeysLastWorkflowApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeysLastWorkflowController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.ReleaseTeks)]
        public async Task<IActionResult> PostWorkflow([FromBody]KeysLastReleaseTeksArgs args, [FromServices]HttpPostKeysLastReleaseTeksCommand command)
        {
            return await command.Execute(args);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.RegisterSecret)]
        public async Task<IActionResult> PostSecret([FromBody]KeysLastSecretArgs args, [FromServices]HttpPostKeysLastRegisterSecret command)
        {
            return await command.Execute(args);
        }
    }
}
