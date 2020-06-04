// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.CdnDataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataController : ControllerBase
    {
        [HttpGet]
        [Route("/manifest")]
        public IActionResult GetCurrentManifest([FromServices] GetLatestManifestCommand command)
        {
            var e = command.Execute();
            var r = new BinaryContentResponse
            {
                LastModified = e.Release,
                PublishingId = e.PublishingId,
                ContentTypeName = e.ContentTypeName,
                Content = e.Content
            };
            return new OkObjectResult(r);
        }

        [HttpGet]
        [Route("/rivmadvice/{id}")]
        public IActionResult GetCurrentManifest(string id, [FromServices]HttpGetContentCommand<ResourceBundleContentEntity> command)
            =>  command.Execute(id);

        [HttpGet]
        [Route("/RiskCalculationconfig/{id}")]
        public IActionResult GetCurrent(string id, [FromServices]HttpGetContentCommand<ResourceBundleContentEntity> command)
            => command.Execute(id);
    }
}
