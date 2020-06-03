// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RivmAdviceController : ControllerBase
    {
        [HttpGet]
        [Route("/rivmadvice/{id}")]
        public IActionResult GetById(string id, [FromServices]HttpGetRivmAdviceCommand command)
        {
            return command.Execute(id);
        }

        [HttpPost]
        [Route("/rivmadvice")]
        public IActionResult Post([FromBody]MobileDeviceRivmAdviceArgs args, [FromServices]HttpPostRivmAdviceCommand command)
        {
            return command.Execute(args);
        }
    }
}