// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi.Services;

namespace NL.Rijksoverheid.ExposureNotification.Icc.v2.WebApi.Controllers
{
    /// <summary>
    /// The PubTEK API
    /// </summary>
    public class WorkflowController : Controller
    {
        private readonly ILogger _logger;

        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// The PubTEK Put service accepting PubTEK's to be published
        /// </summary>
        /// <param name="args"></param>
        /// <param name="publishTekService"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("/pubtek")]
        public async Task<IActionResult> PutPubTek([FromBody] PublishTekArgs args, [FromServices] IPublishTekService publishTekService)
        {
            if (publishTekService == null) throw new ArgumentNullException(nameof(publishTekService));

            _logger.WritePubTekStart();

            var result = await publishTekService.ExecuteAsync(args);

            // As per rfc7231#section-6.3.1 HTTP 200 OK will be returned to indicate that the request has succeeded.
            // Please note that HTTP 200 OK will be returned regardless of whether the key is considered valid or invalid. It may be understood as “request received and processed”.
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
