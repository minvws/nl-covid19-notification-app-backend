// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigController : ControllerBase
    {
        [HttpGet]
        [Route("/config")]
        public IActionResult GetLatestConfig([FromServices]HttpGetLatestManifestCommand command)
        {
            throw new NotImplementedException();
            //return command.Execute();
        }
    }
}
