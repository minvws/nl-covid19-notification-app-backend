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

        //TODO keys last version!
        ///// <summary>
        ///// Generate new WorkFlows.
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //[Route(EndPointNames.DevOps.KeysFirstWorkFlow.TekSetsGenerateRandom)]
        //public async Task<IActionResult> WorkFlows([FromBody]GenerateKeysFirstTekSetsArgs arguments, [FromServices]HttpPostKeysFirstGenerateTekSetsCommand command)
        //{
        //    return await command.Execute(arguments);
        //}

        /// <summary>
        /// Generate new ExposureKeySets.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(EndPointNames.DevOps.ExposureKeySetsCreate + "/{useAllKeys}")]
        public async Task<IActionResult> ExposureKeySets(bool useAllKeys, [FromServices]HttpPostGenerateExposureKeySetsCommand command)
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
