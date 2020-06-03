// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExposureKeySetsController : ControllerBase
    {
        private const string ContentTypeNameJson = "application/json";

        /// <summary>
        /// Swashbuckle has a long-lived bug around not supporting ProducesAttribute so we have to parse the content-type 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/ExposureKeySets/{id}")]
        public IActionResult GetExposureKeySetsAg(string id, [FromServices]HttpGetAgExposureKeySetCommand command)
        {
            var resultFormat = !string.Equals(Request.ContentType, ContentTypeNameJson, StringComparison.InvariantCultureIgnoreCase) 
                ? ExposureKeySetFormat.Ag : ExposureKeySetFormat.Json;

            return null; //TODO command.Execute(id, resultFormat);
        }
    }
}
