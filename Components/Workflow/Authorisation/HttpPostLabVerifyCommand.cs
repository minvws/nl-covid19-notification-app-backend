// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class HttpPostLabVerifyCommand
    {
        private readonly LabVerifyArgsValidator _LabVerifyArgsValidator;
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly ILogger<HttpPostLabVerifyCommand> _Logger;
        private readonly WriteNewPollTokenWriter _Writer;

        public HttpPostLabVerifyCommand(LabVerifyArgsValidator labVerifyArgsValidator, WorkflowDbContext dbContextProvider, ILogger<HttpPostLabVerifyCommand> logger, WriteNewPollTokenWriter writer)
        {
            _LabVerifyArgsValidator = labVerifyArgsValidator ?? throw new ArgumentNullException(nameof(labVerifyArgsValidator));
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public async Task<IActionResult> Execute(LabVerifyArgs args)
        {
            if (!_LabVerifyArgsValidator.Validate(args))
                return new BadRequestResult();

            var wf = await _DbContextProvider.KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .SingleOrDefaultAsync(x => x.PollToken == args.PollToken);

            if (wf == null)
            {
                _Logger.LogError("KeyReleaseWorkflowState not found - PollToken:{PollToken}.", args.PollToken);
                var error = new LabVerifyAuthorisationResponse {Error = "Workflow not found.", Valid = false};
                return new OkObjectResult(error);
            }

            var result =  new LabVerifyAuthorisationResponse
            {
                PollToken = _Writer.Execute(wf),
                Valid = wf.Teks?.Count != 0
            };

            return new OkObjectResult(result);
        }
    }
}