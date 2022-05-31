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
    public class ExposureKeySetController : ControllerBase
    {
        [HttpGet]
        [Route("/v4/exposurekeyset/{id}")]
        public async Task GetExposureKeySetV2Async(string id, [FromServices] HttpGetCdnEksCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ExposureKeySetV2, id);
        }

        [HttpGet]
        [Route("/v5/exposurekeyset/{id}")]
        public async Task GetExposureKeySetAsync(string id, [FromServices] HttpGetCdnEksCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ExposureKeySetV3, id);
        }
    }
}
