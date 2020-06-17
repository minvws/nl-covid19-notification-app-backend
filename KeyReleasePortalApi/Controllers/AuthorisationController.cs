// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.KeyReleasePortalApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthorisationController : ControllerBase
    {
        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody]KeysLastAuthorisationArgs args, [FromServices]HttpPostKeysLastAuthorise command)
        {
            return await command.Execute(args);
        }
    }
}
