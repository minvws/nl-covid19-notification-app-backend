// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : ControllerBase
    {

        /// <summary>
        /// TODO replace the DynamicManifestReader version with the static reader.
        /// </summary>
        //[HttpGet]
        //[Route(EndPointNames.CdnApi.Manifest)]
        //public async Task GetManifest([FromServices] HttpGetCdnManifestCommand command)
        //{
        //    await command.Execute(HttpContext);
        //}


        [HttpGet]
        [Route(EndPointNames.ContentApi.Manifest)]
        public async Task GetCurrentManifest([FromServices] DynamicManifestReader command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            //logger.LogInformation("GET manifest triggered.");
            var contentEntity = await command.Execute();

            var content = contentEntity.SignedContent;
            HttpContext.Response.Headers.Add("Content-Type", MediaTypeNames.Application.Zip);
            HttpContext.Response.Headers.Add("CacheControl", "s-maxage=14400;etc");
            HttpContext.Response.ContentLength = content.Length;
            await HttpContext.Response.Body.WriteAsync(content);
        }

        //private async Task<IActionResult> HttpPostManifest([FromBody] ReceiveContentArgs args,
        //    [FromServices] ManifestBlobWriter blobWriter,
        //    [FromServices] IQueueSender<StorageAccountSyncMessage> qSender,
        //    [FromServices] ILogger<HttpPostContentReceiver2> logger)
        //{
        //    if (args == null) throw new ArgumentNullException(nameof(args));
        //    if (blobWriter == null) throw new ArgumentNullException(nameof(blobWriter));
        //    if (qSender == null) throw new ArgumentNullException(nameof(qSender));
        //    if (logger == null) throw new ArgumentNullException(nameof(logger));

        //    var command = new HttpPostContentReceiver2(blobWriter, qSender, logger);
        //    return await command.Execute(args, new DestinationArgs { Path = _ContentPathProvider.Manifest, Name = EndPointNames.ManifestName });
        //}


        [HttpGet]
        [Route(EndPointNames.ContentApi.AppConfig + "/{id}")]
        public async Task GetAppConfig(string id, [FromServices] HttpGetCdnContentCommand<AppConfigContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

        [HttpGet]
        [Route(EndPointNames.ContentApi.RiskCalculationParameters + "/{id}")]
        public async Task GetRiskCalculationParameters(string id, [FromServices]HttpGetCdnContentCommand<RiskCalculationContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

        [HttpGet]
        [Route(EndPointNames.ContentApi.ExposureKeySet + "/{id}")]
        public async Task GetExposureKeySet(string id, [FromServices]HttpGetCdnContentCommand<ExposureKeySetContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }
    }
}
