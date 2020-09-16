// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentControllerV2 : ControllerBase
    {
        [HttpGet]
        [Route("/v2/manifest")]
        public async Task GetManifest([FromServices] HttpGetCdnManifestCommand command)
        {
            await command.Execute(HttpContext, ContentTypes.ManifestV2);
        }

        [HttpGet]
        [Route("/v2/appconfig/{id}")]
        public async Task GetAppConfig(string id, [FromServices] HttpGetCdnContentCommand command)
        {
            await command.Execute(HttpContext, ContentTypes.AppConfigV2, id);
        }

        [HttpGet]
        [Route("/v2/riskcalculationparameters/{id}")]
        public async Task GetRiskCalculationParameters(string id, [FromServices] HttpGetCdnContentCommand command)
        {
            await command.Execute(HttpContext, ContentTypes.RiskCalculationParametersV2, id);
        }

        [HttpGet]
        [Route("/v2/exposurekeyset/{id}")]
        public async Task GetExposureKeySet(string id, [FromServices] HttpGetCdnContentCommand command)
        {
            await command.Execute(HttpContext, ContentTypes.ExposureKeySetV2, id);
        }
    }
}
