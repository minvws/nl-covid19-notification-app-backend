// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.WebApi.Controllers
{
    [ApiController]
    public class RiskCalculationParametersController : ControllerBase
    {
        [HttpGet]
        [Route("/v1/riskcalculationparameters/{id}")]
        public async Task GetRiskCalculationParametersV1Async(string id, [FromServices] HttpGetCdnImmutableNonExpiringContentCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.RiskCalculationParameters, id);
        }

        [HttpGet]
        [Route("/v2/riskcalculationparameters/{id}")]
        [Route("/v3/riskcalculationparameters/{id}")]
        public async Task GetRiskCalculationParametersV2Async(string id, [FromServices] HttpGetCdnImmutableNonExpiringContentCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.RiskCalculationParametersV2, id);
        }

        [HttpGet]
        [Route("/v4/riskcalculationparameters/{id}")]
        public async Task GetRiskCalculationParametersAsync(string id, [FromServices] HttpGetCdnImmutableNonExpiringContentCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.RiskCalculationParametersV3, id);
        }
    }
}
