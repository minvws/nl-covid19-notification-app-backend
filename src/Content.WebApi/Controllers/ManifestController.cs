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
    public class ManifestController : ControllerBase
    {
        [HttpGet]
        [Route("/v2/manifest")]
        public async Task GetManifestV2Async([FromServices] HttpGetCdnManifestCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ManifestV2);
        }

        [HttpGet]
        [Route("/v3/manifest")]
        public async Task GetManifestV3Async([FromServices] HttpGetCdnManifestCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ManifestV3);
        }

        [HttpGet]
        [Route("/v4/manifest")]
        public async Task GetManifestV4Async([FromServices] HttpGetCdnManifestCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ManifestV4);
        }

        [HttpGet]
        [Route("/v5/manifest")]
        public async Task GetManifestAsync([FromServices] HttpGetCdnManifestCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ManifestV5);
        }
    }
}
