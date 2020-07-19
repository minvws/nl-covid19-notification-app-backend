// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content.NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : ControllerBase
    {
        [HttpGet]
        [Route(EndPointNames.ContentApi.Manifest)]
        public async Task GetManifest([FromServices] HttpGetCdnManifestCommand command)
        {
            await command.Execute(HttpContext);
        }

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
