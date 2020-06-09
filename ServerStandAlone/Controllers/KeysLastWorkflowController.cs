// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeysLastWorkflowController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.ReleaseTeks)]
        public IActionResult PostWorkflow([FromBody]KeysLastReleaseTeksArgs args, [FromServices]HttpPostKeysLastReleaseTeksCommand command)
        {
            return command.Execute(args);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.RegisterSecret)]
        public IActionResult PostSecret([FromBody]KeysLastSecretArgs args, [FromServices]HttpPostKeysLastRegisterSecret command)
        {
            return command.Execute(args);
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.KeysLastWorkflow.Authorise)]
        public IActionResult PostAuthorise([FromBody]KeysLastAuthorisationArgs args, [FromServices]HttpPostKeysLastAuthorise command)
        {
            return command.Execute(args);
        }
    }
}
