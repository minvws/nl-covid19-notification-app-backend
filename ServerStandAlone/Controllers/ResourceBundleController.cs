// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceBundleController : ControllerBase
    {
        [HttpGet]
        [Route(EndPointNames.CdnApi.ResourceBundle + "/{id}")]
        [ProducesResponseType(typeof(ResourceBundleContent), 200)]
        //[ProducesResponseType(typeof(byte[]), 200)] //TODO No added value in Swagger UI
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces(MediaTypeNames.Application.Zip, MediaTypeNames.Application.Json)]
        public async Task GetLatestConfig(string id, [FromServices]HttpGetCdnContentCommand<ResourceBundleContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

        [HttpPost]
        [Route(EndPointNames.ContentAdminPortalDataApi.ResourceBundle)]
        [ProducesResponseType(500)]
        [ProducesResponseType(400)]
        [Consumes(MediaTypeNames.Application.Json)]
        public async Task<IActionResult> Post([FromBody]ResourceBundleArgs args, [FromServices]HttpPostResourceBundleCommand command)
        {
            return await command.Execute(args);
        }
    }
}