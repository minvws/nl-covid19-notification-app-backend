// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RootController : ControllerBase
    {
        [HttpGet]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}