// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.CdnDataApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class DataController : ControllerBase
    {
        [HttpGet]
        [Route(EndPointNames.CdnApi.Manifest)]
        public async Task<IActionResult> GetCurrentManifest([FromServices]HttpGetManifestBinaryContentCommand command)
            => await command.Execute(HttpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.ExposureKeySet + "/{id}")]
        public async Task<IActionResult> GetExposureKeySet(string id, [FromServices]HttpGetBinaryContentCommand<ExposureKeySetContentEntity> command)
            => await command.Execute(id, HttpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.ResourceBundle + "/{id}")]
        public async Task<IActionResult> GetResourceBundle(string id, [FromServices]HttpGetBinaryContentCommand<ResourceBundleContentEntity> command)
            => await command.Execute(id, HttpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.RiskCalculationParameters + "/{id}")]
        public async Task<IActionResult> GetRiskCalculationParameters(string id, [FromServices] HttpGetBinaryContentCommand<RiskCalculationContentEntity> command)
            => await command.Execute(id, HttpContext);

        [HttpGet]
        [Route(EndPointNames.CdnApi.AppConfig + "/{id}")]
        public async Task<IActionResult> GetAppConfig(string id, [FromServices] HttpGetBinaryContentCommand<AppConfigContentEntity> command)
            => await command.Execute(id, HttpContext);
    }
}
