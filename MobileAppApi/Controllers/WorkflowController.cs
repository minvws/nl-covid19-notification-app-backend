// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.PostKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DecoyKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkflowController : ControllerBase
    {
        private readonly LoggingExtensionsRegisterSecret _LoggerRegisterSecret;
        private readonly LoggingExtensionsPostKeys _LoggerPostKeys;
        private readonly LoggingExtensionsDecoyKeys _LoggerDecoyKeys;

        public WorkflowController(LoggingExtensionsRegisterSecret loggerRegisterSecret, LoggingExtensionsPostKeys loggerPostKeys, LoggingExtensionsDecoyKeys LoggerDecoyKeys)
        {
            _LoggerRegisterSecret = loggerRegisterSecret ?? throw new ArgumentNullException(nameof(loggerRegisterSecret));
            _LoggerPostKeys = loggerPostKeys ?? throw new ArgumentNullException(nameof(loggerPostKeys));
            _LoggerDecoyKeys = LoggerDecoyKeys ?? throw new ArgumentNullException(nameof(LoggerDecoyKeys));
        }

        [ResponsePaddingFilterFactory]
        [SuppressErrorFactory]
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.ReleaseTeks)]
        public async Task<IActionResult> PostWorkflow([FromQuery] byte[] sig, [FromServices] HttpPostReleaseTeksCommand2 command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _LoggerPostKeys.WriteStartPostKeys();
            return await command.Execute(sig, Request);
        }

        [ResponsePaddingFilterFactory]
        [SuppressErrorFactory]
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.Register)]
        public async Task<IActionResult> PostSecret([FromServices]HttpPostRegisterSecret command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            _LoggerRegisterSecret.WriteStartSecret();
            return await command.Execute();
        }

        [ResponsePaddingFilterFactory]
        [SuppressErrorFactory]
        [DecoyTimeGeneratorAttributeFactory]
        [HttpPost]
        [Route(EndPointNames.MobileAppApi.RandomNoise)]
        public async Task<IActionResult> StopKeys()
        {
            _LoggerDecoyKeys.WriteStartDecoy();
            return new OkResult();
        }

        [HttpGet]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());
    }
}
