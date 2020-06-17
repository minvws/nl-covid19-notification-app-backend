// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.KeysLastWorkflowApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeysLastWorkflowController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.ReleaseTeks)]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostKeysLastReleaseTeksCommand command)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            return await command.Execute(sig, body);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.RegisterSecret)]
        public async Task<IActionResult> PostSecret([FromBody]KeysLastSecretArgs args, [FromServices]HttpPostKeysLastRegisterSecret command)
        {
            return await command.Execute(args);
        }
    }
}
