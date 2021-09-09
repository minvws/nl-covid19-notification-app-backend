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
    public class ResourceBundleController : ControllerBase
    {
        [HttpGet]
        [Route("/v2/resourcebundle/{id}")]
        public async Task GetResourceBundleV2Async(string id, [FromServices] HttpGetCdnImmutableNonExpiringContentCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ResourceBundleV2, id);
        }

        [HttpGet]
        [Route("/v3/resourcebundle/{id}")]
        [Route("/v4/resourcebundle/{id}")]
        public async Task GetResourceBundleAsync(string id, [FromServices] HttpGetCdnImmutableNonExpiringContentCommand command)
        {
            await command.ExecuteAsync(HttpContext, ContentTypes.ResourceBundleV3, id);
        }
    }
}
