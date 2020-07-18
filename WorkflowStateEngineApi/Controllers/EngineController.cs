// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.BackgroundJobs;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.WorkflowStateEngineApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class EngineController : ControllerBase
    {
        private readonly ILogger _Logger;
        public EngineController(ILogger logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet] //TODO POST!
        [Route("/v1/execute")]
        public async Task<IActionResult> Purge([FromServices] PurgeExpiredSecretsDbCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.LogInformation("GET v1/execute to Purge Expired Secrets.");
            return await command.Execute();
        }
    }
}
