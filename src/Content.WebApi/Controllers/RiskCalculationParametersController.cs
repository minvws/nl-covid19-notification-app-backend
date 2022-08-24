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
        [Route("/v6/riskcalculationparameters/{id}")]
        public async Task GetRiskCalculationParametersV3Async(string id, [FromServices] HttpGetCdnImmutableNonExpiringContentCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.RiskCalculationParameters, id);
        }
    }
}
