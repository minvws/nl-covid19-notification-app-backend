// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.BatchJobsApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EngineController : ControllerBase
    {
        private readonly ILogger _Logger;

        public EngineController(ILogger<EngineController> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        [Route("/v1/eksengine")]
        public async Task<IActionResult> ExposureKeySets([FromQuery] bool useAllKeys, [FromServices] ExposureKeySetBatchJobMk2 command)
        {
            _Logger.LogInformation("EKS Engine triggered.");
            await command.Execute(useAllKeys);
            return new OkResult();
        }

        [HttpPost]
        [Route("/v1/manifestengine")]
        public async Task<IActionResult> Manifest([FromServices] ManifestBatchJob command)
        {
            _Logger.LogInformation("Manifest Engine triggered.");
            await command.Execute();
            return new OkResult();
        }

        [HttpPost]
        [Route("/v1/nukeandpavedb")]
        public async Task<IActionResult> NakeAndPaveDb([FromServices] ProvisionDatabasesCommand command)
        {
            _Logger.LogInformation("Provision Databases triggered.");
            await command.Execute(new string[0]);
            return new OkResult();
        }
    }
}
