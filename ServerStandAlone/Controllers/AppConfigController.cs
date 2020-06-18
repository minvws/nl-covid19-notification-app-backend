// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AppConfigController : ControllerBase
    {
        /// <summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(EndPointNames.CdnApi.AppConfig +"/{id}")]
        [ProducesResponseType(typeof(AppConfigContent), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces(MediaTypeNames.Application.Zip, MediaTypeNames.Application.Json)]
        public async Task GetLatestConfig(string id, [FromServices]HttpGetCdnContentCommand<AppConfigContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

        [HttpPost]
        [Route(EndPointNames.ContentAdminPortalDataApi.AppConfig)]
        public async Task<IActionResult> Post([FromBody]AppConfigArgs args, [FromServices]HttpPostAppConfigCommand command)
        {
            return await command.Execute(args);
        }
    }
}
