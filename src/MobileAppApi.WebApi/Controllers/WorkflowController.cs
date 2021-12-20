// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Controllers
{
    [ApiController]
    public class WorkflowController : ControllerBase
    {
        private readonly ILogger _logger;

        public WorkflowController(ILogger<WorkflowController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [ResponsePaddingFilterFactory]
        [SuppressErrorFactory]
        [DecoyTimeAggregatorAttributeFactory]
        [HttpPost]
        [Route("/v1/postkeys")]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostReleaseTeksCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            _logger.LogInformation("POST triggered.");
            return await command.ExecuteAsync(sig, Request);
        }

        [ResponsePaddingFilterFactory]
        [SuppressErrorFactory]
        [DecoyTimeAggregatorAttributeFactory]
        [HttpPost]
        [Route("/v2/register")]
        public async Task<IActionResult> PostSecret([FromServices] HttpPostRegisterSecret command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            return await command.ExecuteAsync();
        }

        [ResponsePaddingFilterFactory]
        [SuppressErrorFactory]
        [DecoyTimeGeneratorAttributeFactory]
        [HttpPost]
        [Route("/v1/stopkeys")]
        public IActionResult StopKeys()
        {
            _logger.LogInformation("POST triggered.");
            return new OkResult();
        }

        [HttpGet]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
