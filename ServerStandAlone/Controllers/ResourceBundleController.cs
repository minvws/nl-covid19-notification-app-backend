// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResourceBundleController : ControllerBase
    {
        [HttpGet]
        [Route(EndPointNames.CdnApi.ResourceBundle + "/{id}")]
        public async Task GetLatestConfig(string id, [FromServices]HttpGetCdnContentCommand<ResourceBundleContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

        [HttpPost]
        [Route(EndPointNames.CdnApi.ResourceBundle)]
        public IActionResult Post([FromBody]ResourceBundleArgs args, [FromServices]HttpPostResourceBundleCommand command)
        {
            return command.Execute(args);
        }
    }
}