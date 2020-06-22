// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EKSEngineApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AuthorisationController : ControllerBase
    {
        /// <summary>
        /// Generate new ExposureKeySets.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(EndPointNames.DevOps.ExposureKeySetsCreate)]
        public async Task<IActionResult> ExposureKeySets([FromServices]HttpPostGenerateExposureKeySetsCommand command)
        {
            return await command.Execute();
        }
    }
}
