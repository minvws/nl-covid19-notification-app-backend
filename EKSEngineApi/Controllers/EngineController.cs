// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EKSEngineApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class EngineController : ControllerBase
    {
        private readonly ILogger _Logger;

        public EngineController(ILogger<EngineController> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate new ExposureKeySets.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/v1/execute")]
        public async Task<IActionResult> ExposureKeySets([FromQuery] bool useAllKeys, [FromServices] HttpPostGenerateExposureKeySetsCommand command)
        {
            _Logger.LogInformation("EKS Engine triggered.");
            return await command.Execute(useAllKeys);
        }
    }
}
