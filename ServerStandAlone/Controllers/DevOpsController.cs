// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.BackgroundJobs;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevOpsController : ControllerBase
    {
        /// <summary>
        /// Provision Database.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(EndPointNames.DevOps.NukeAndPave)]
        public async Task<IActionResult> ProvisionDb([FromServices]ProvisionDatabasesCommand command)
        {
            return await command.Execute();
        }
        
        /// <summary>
        /// Provision Database for IccBackend
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        
        [HttpPost]
        [Route(EndPointNames.DevOps.NukeAndPaveIcc)]
        public async Task<IActionResult> ProvisionDb([FromServices]ProvisionDatabasesCommandIcc command)
        {
            return await command.Execute();
        }

        /// <summary>
        /// Generate new ExposureKeySets.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(EndPointNames.DevOps.ExposureKeySetsCreate)]
        public async Task<IActionResult> ExposureKeySets([FromQuery]bool useAllKeys, [FromServices]HttpPostGenerateExposureKeySetsCommand command)
        {
            return await command.Execute(useAllKeys);
        }

        /// <summary>
        /// Delete all keysets which are invalid
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Purge")]
        public async Task<IActionResult> Purge([FromServices] PurgeExpiredSecretsDbCommand command)
        {
            return await command.Execute();
        }
    }
}
