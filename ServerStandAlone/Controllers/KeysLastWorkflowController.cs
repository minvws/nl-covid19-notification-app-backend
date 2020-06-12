// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysFirstWorkflow.EscrowTeks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow.SendTeks;
using Swashbuckle.AspNetCore.Annotations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KeysLastWorkflowController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.ReleaseTeks)]
        [SwaggerOperation(
            Summary = "Creates a new product",
            Description = "Requires admin privileges",
            OperationId = "CreateProduct",
            Tags = new[] { "Purchase", "Products" }
        )]
        public async Task<IActionResult> PostWorkflow([FromBody]KeysLastReleaseTeksArgs args, [FromServices]HttpPostKeysLastReleaseTeksCommand command)
        {
            return await command.Execute(args);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.KeysLastWorkflow.RegisterSecret)]
        [SwaggerOperation(
            Summary = "Creates a new product",
            Description = "Requires admin privileges",
            OperationId = "CreateProduct",
            Tags = new[] { "Purchase", "Products" }
        )]
        public async Task<IActionResult> PostSecret([FromBody]KeysLastSecretArgs args, [FromServices]HttpPostKeysLastRegisterSecret command)
        {
            return await command.Execute(args);
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.KeysLastWorkflow.LabConfirmation)]
        [SwaggerOperation(
            Summary = "Creates a new product",
            Description = "Requires admin privileges",
            OperationId = "CreateProduct",
            Tags = new[] { "Purchase", "Products" }
        )]
        public async Task<IActionResult> PostAuthorise([FromBody]KeysLastAuthorisationArgs args, [FromServices]HttpPostKeysLastAuthorise command)
        {
            return await command.Execute(args);
        }
    }
}
