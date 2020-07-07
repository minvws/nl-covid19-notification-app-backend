// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExposureKeySetsController : ControllerBase
    {
        [HttpGet]
        [Route(EndPointNames.CdnApi.ExposureKeySet + "/{id}")]
        [ProducesResponseType(typeof(TemporaryExposureKeyExport), 200)]
        [ProducesResponseType(500)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [Produces(MediaTypeNames.Application.Zip, MediaTypeNames.Application.Json)]
        public async Task GetLatestConfig(string id, [FromServices] HttpGetSignedCdnContentOnlyCommand<ExposureKeySetContentEntity> command)
        {
            await command.Execute(HttpContext, id);
        }

    }
}
