// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Applications.CdnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContentController : ControllerBase
    {
        public ContentController(ILogger<ContentController> logger)
        {
            logger.LogInformation("CdnApi.Controllers init");
        }

        [HttpGet]
        [Route("/manifest")]
        public async Task GetCurrentManifest()
        {
            var r = await new CdnContentHttpReader().Execute("/manifest");
            new HttpGetCdnContentCommand().Execute(HttpContext, r);
        }

        [HttpGet]
        [Route("/rivmadvice/{id}")]
        public async Task GetCurrentManifest(string id)
        {
            var r = await new CdnContentHttpReader().Execute("/rivmadvice", id);
            new HttpGetCdnContentCommand().Execute(HttpContext, r);
        }

        [HttpGet]
        [Route("/RiskCalculationconfig/{id}")]
        public async Task GetCurrent(string id)
        {
            var r = await new CdnContentHttpReader().Execute("/RiskCalculationconfig", id);
            new HttpGetCdnContentCommand().Execute(HttpContext, r);
        }
    }
}
