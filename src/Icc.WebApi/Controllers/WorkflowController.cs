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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation.Handlers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;
using NL.Rijksoverheid.ExposureNotification.Icc.WebApi.Services;

namespace NL.Rijksoverheid.ExposureNotification.Icc.WebApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtAuthenticationHandler.SchemeName)]
    public class WorkflowController : Controller
    {
        private const int OldGGDKeyLength = 6;
        private const int ValidGGDKeyLength = 7;

        private readonly ILogger _Logger;
        private readonly ILuhnModNGenerator _lLuhnModNGenerator;
        private readonly ILuhnModNValidator _luhnModNValidator;

        public WorkflowController(ILogger<WorkflowController> logger, ILuhnModNGenerator luhnModNGenerator, ILuhnModNValidator luhnModNValidator)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lLuhnModNGenerator = luhnModNGenerator ?? throw new ArgumentNullException(nameof(luhnModNGenerator));
            _luhnModNValidator = luhnModNValidator ?? throw new ArgumentNullException(nameof(luhnModNValidator));
        }

        [HttpPost]
        [Route(EndPointNames.CaregiversPortalApi.LabConfirmation)]
        public async Task<IActionResult> PostAuthorise([FromBody] AuthorisationArgs args, [FromServices] HttpPostAuthoriseCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _Logger.WriteLabStart();
            return await command.ExecuteAsync(args);
        }

        /// <summary>
        /// The PubTEK Put service accepting PubTEK's to be published
        /// </summary>
        /// <param name="args"></param>
        /// <param name="publishTekService"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("/pubtek")]
        public async Task<PublishTekResponse> PutPubTek([FromBody] PublishTekArgs args, [FromServices] IPublishTekService publishTekService)
        {
            if (publishTekService == null) throw new ArgumentNullException(nameof(publishTekService));

            if (!FixOrValidatePubTEK(args))
            {
                return new PublishTekResponse { Valid = false };
            }

            _Logger.WritePubTekStart();

            var result = await publishTekService.ExecuteAsync(args);

            // As per rfc7231#section-6.3.1 HTTP 200 OK will be returned to indicate that the request has succeeded.
            // Please note that HTTP 200 OK will be returned regardless of whether the key is considered valid or invalid. It may be understood as “request received and processed”.
            return result;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/")]
        public IActionResult AssemblyDump([FromServices] IWebHostEnvironment env) => new DumpAssembliesToPlainText().Execute(env.IsDevelopment());

        private bool FixOrValidatePubTEK(PublishTekArgs args)
        {
            // If a 6 digit code has been send no LuhnModN validation is possible at this point. Just add the check code and return valid.
            if (args.GGDKey.Length == OldGGDKeyLength)
            {
                args.GGDKey = _lLuhnModNGenerator.CalculateCheckCode(args.GGDKey);
                return true;
            }

            // Else the code should be 7 digits and validated.
            return args.GGDKey.Length == ValidGGDKeyLength && _luhnModNValidator.Validate(args.GGDKey);
        }
    }
}
