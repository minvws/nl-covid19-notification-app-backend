// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;
using Swashbuckle.AspNetCore.Annotations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.ReleaseTeks)]
        [SwaggerOperation(
            Summary = "Creates a new product",
            Description = "Requires admin privileges",
            OperationId = "CreateProduct",
            Tags = new[] { "Purchase", "Products" }
        )]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostReleaseTeksCommand command)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            return await command.Execute(sig, body);
        }

        [HttpPost]
        [Route(EndPointNames.MobileAppApi.RegisterSecret)]
        [SwaggerOperation(
            Summary = "Creates a new product",
            Description = "Requires admin privileges",
            OperationId = "CreateProduct",
            Tags = new[] { "Purchase", "Products" }
        )]
        public async Task<IActionResult> PostSecret([FromBody] SecretArgs args, [FromServices] HttpPostRegisterSecret command)
        {
            return await command.Execute(args);
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        [SwaggerOperation(
            Summary = "Creates a new product",
            Description = "Requires admin privileges",
            OperationId = "CreateProduct",
            Tags = new[] { "Purchase", "Products" }
        )]
        public async Task<IActionResult> PostAuthorise([FromBody] AuthorisationArgs args, [FromServices] HttpPostAuthorise command)
        {
            return await command.Execute(args);
        }
    }
}
