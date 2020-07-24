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
        private readonly PollTokens _PollTokens;
        private readonly ILogger _Logger;

        public LabVerificationAuthorisationCommand(WorkflowDbContext dbContextProvider, PollTokens pollTokens,
            ILogger<LabVerificationAuthorisationCommand> logger)
        {
            _DbContextProvider = dbContextProvider;
            _PollTokens = pollTokens;
            _Logger = logger;
        }

        public bool Validate(LabVerifyArgs args)
        {
            if (args == null)
                return false;

            if (string.IsNullOrWhiteSpace(args.PollToken))
                return false;

            //
            // if(_PollTokens.)
            
            try
            {
                return _PollTokens.Validate(args.PollToken);
            }
            catch (TokenExpiredException e)
            {
                return false;
            }
        }

        public async Task<LabVerifyAuthorisationResponse> Execute(LabVerifyArgs args)
        {
            if (!_PollTokens.Validate(args.PollToken))
                throw new ArgumentException("Not valid.", nameof(args));

            var wf = await _DbContextProvider.KeyReleaseWorkflowStates
                .Include(x => x.Keys)
                .FirstOrDefaultAsync(state =>
                    state.PollToken == args.PollToken);

            if (wf == null)
            {
                var message = $"KeyReleaseWorkflowState not found - PollToken:{args.PollToken}.";
                _Logger.LogError(message);
                return new LabVerifyAuthorisationResponse {Error = "Workflow not found.", Valid = false};
            }

            var refreshedToken = _PollTokens.GenerateToken();
            wf.PollToken = refreshedToken;
            _Logger.LogDebug($"Committing.");
            _DbContextProvider.SaveAndCommit();

            _Logger.LogInformation($"Committed - new PollToken:{wf.PollToken}.");
            return new LabVerifyAuthorisationResponse
                {PollToken = refreshedToken, Valid = wf.Keys?.Any() ?? false};
        }
    }
}