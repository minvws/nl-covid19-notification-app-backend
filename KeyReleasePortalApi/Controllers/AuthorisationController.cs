// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.KeyReleaseApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AuthorisationController : ControllerBase
    {
        [HttpPost]
        [Route("/v1/labconfirm")]
        public async Task<IActionResult> PostAuthorise([FromBody]AuthorisationArgs args, [FromServices]HttpPostAuthorise command)
        {
            return await command.Execute(args);
        }
    }
}
