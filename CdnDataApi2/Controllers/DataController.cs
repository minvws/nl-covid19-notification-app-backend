// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.CdnDataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        [HttpGet]
        [Route(EndPointNames.CdnApi.Manifest)]
        [Produces("application/x-protobuf")]
        [ProducesResponseType(200)]
        //[ProducesResponseType(typeof(byte[]), 200)] //TODO No added value in Swagger UI
        [ProducesResponseType(500)]
        public async Task GetCurrentManifest([FromServices] DynamicManifestReader command, HttpContext httpContext)
        {
            var e = await command.Execute();

            var r = new BinaryContentResponse
            {
                LastModified = e.Release,
                PublishingId = e.PublishingId,
                ContentTypeName = e.ContentTypeName,
                Content = e.Content,
                SignedContentTypeName = e.SignedContentTypeName,
                SignedContent = e.SignedContent
            };

            await httpContext.RespondWith(r);
        }

        [HttpGet]
        [Route(EndPointNames.CdnApi.ExposureKeySet + "/{id}")]
        public async Task GetExposureKeySet(string id, [FromServices]HttpGetBinaryContentCommand<ExposureKeySetContentEntity> command, HttpContext httpContext)
            => await command.Execute(id, httpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.ResourceBundle + "/{id}")]
        public async Task GetResourceBundle(string id, [FromServices]HttpGetBinaryContentCommand<ResourceBundleContentEntity> command, HttpContext httpContext)
            => await command.Execute(id, httpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters + "/{id}")]
        public async Task GetRiskCalculationParameters(string id, [FromServices] HttpGetBinaryContentCommand<RiskCalculationContentEntity> command, HttpContext httpContext)
            => await command.Execute(id, httpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.AppConfig + "/{id}")]
        public async Task GetAppConfig(string id, [FromServices] HttpGetBinaryContentCommand<AppConfigContentEntity> command, HttpContext httpContext)
            => await command.Execute(id, httpContext);
    }
}
