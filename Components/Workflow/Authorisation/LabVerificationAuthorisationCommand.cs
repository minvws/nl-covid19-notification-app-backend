// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using JWT.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class LabVerificationAuthorisationCommand
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenService _PollTokenService;
        private readonly ILogger _Logger;

        public LabVerificationAuthorisationCommand(WorkflowDbContext dbContextProvider, PollTokenService pollTokenService,
            ILogger<LabVerificationAuthorisationCommand> logger)
        {
            _DbContextProvider = dbContextProvider;
            _PollTokenService = pollTokenService;
            _Logger = logger;
        }
        public async Task<LabVerifyAuthorisationResponse> Execute(LabVerifyArgs args)
        {
            var wf = await _DbContextProvider.KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .FirstOrDefaultAsync(state =>
                    state.PollToken == args.PollToken);

            if (wf == null)
            {
                var message = $"KeyReleaseWorkflowState not found - PollToken:{args.PollToken}.";
                _Logger.LogError(message);
                return new LabVerifyAuthorisationResponse {Error = "Workflow not found.", Valid = false};
            }

            var refreshedToken = _PollTokenService.GenerateToken();
            wf.PollToken = refreshedToken;
            _Logger.LogDebug($"Committing.");
            _DbContextProvider.SaveAndCommit();

            _Logger.LogInformation($"Committed - new PollToken:{wf.PollToken}.");
            return new LabVerifyAuthorisationResponse
                {PollToken = refreshedToken, Valid = wf.Teks?.Any() ?? false};
        }
    }
}