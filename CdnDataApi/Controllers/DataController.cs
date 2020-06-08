// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
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
        //TODO [Obsolete("Move to the CdnApi in Azure?")]
        //[HttpGet]
        //[Route(EndPointNames.CdnApi.Manifest)]
        //public IActionResult GetCurrentManifest([FromServices] GetLatestManifestCommand command)
        //{
        //    var e = command.Execute();
        //    var r = new BinaryContentResponse
        //    {
        //        LastModified = e.Release,
        //        PublishingId = e.PublishingId,
        //        ContentTypeName = e.ContentTypeName,
        //        Content = e.Content
        //    };
        //    return new OkObjectResult(r);
        //}

        [HttpGet]
        [Route(EndPointNames.CdnApi.ExposureKeySet + "/{id}")]
        public IActionResult GetExposureKeySet(string id, [FromServices]HttpGetBinaryContentCommand<ExposureKeySetContentEntity> command)
            => command.Execute(id);

        [HttpGet]
        [Route(EndPointNames.CdnApi.ResourceBundle + "/{id}")]
        public IActionResult GetResourceBundle(string id, [FromServices]HttpGetBinaryContentCommand<ResourceBundleContentEntity> command)
            => command.Execute(id);

        [HttpGet]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters + "/{id}")]
        public IActionResult GetRiskCalculationParameters(string id, [FromServices]HttpGetBinaryContentCommand<RiskCalculationContentEntity> command)
            => command.Execute(id);
    }
}
