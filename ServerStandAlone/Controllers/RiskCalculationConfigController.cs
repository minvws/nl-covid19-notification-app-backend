// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RiskCalculationConfigController : ControllerBase
    {
        [HttpGet]
        [Route("/RiskCalculationconfig/{id}")]
        public IActionResult GetLatestConfig(string id, [FromServices]HttpGetRiskCalculationConfigCommand command)
        {
            return command.Execute(id);
        }

        [HttpPost]
        [Route("/RiskCalculationconfig")]
        public async Task<IActionResult> Post([FromBody]RiskCalculationConfigArgs args, [FromServices]HttpPostRiskCalculationConfigCommand command)
        {
            return await command.Execute(args);
        }
    }
}
