// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class AuthorisationWriterCommand
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenService _PollTokenService;
        private readonly ILogger _Logger;
        private readonly AuthorisationArgsValidator _AuthorisationArgsValidator;

        public AuthorisationWriterCommand(WorkflowDbContext dbContextProvider, PollTokenService pollTokenService,
            ILogger<AuthorisationWriterCommand> logger, AuthorisationArgsValidator authorisationArgsValidator)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _PollTokenService = pollTokenService ?? throw new ArgumentNullException(nameof(pollTokenService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _AuthorisationArgsValidator = authorisationArgsValidator ??
                                          throw new ArgumentNullException(nameof(authorisationArgsValidator));
        }


        public async Task<AuthorisationResponse> Execute(AuthorisationArgs args)
        {
            if (args == null) 
                throw new ArgumentNullException(nameof(args));
            if (!_AuthorisationArgsValidator.Validate(args))
                throw new ArgumentException("Not valid.", nameof(args));

            var wf = await _DbContextProvider
                .KeyReleaseWorkflowStates
                .Include(x => x.Keys)
                .FirstOrDefaultAsync(x => x.LabConfirmationId == args.LabConfirmationId);

            if (wf == null)
            {
                var message = $"KeyReleaseWorkflowState not found - LabConfirmationId:{args.LabConfirmationId}.";
                _Logger.LogError(message);
                return new AuthorisationResponse {Valid = false};
            }

            wf.AuthorisedByCaregiver = true; //TODO but wf.LabConfirmationId = null will suffice?
            wf.Authorised = wf.Keys?.Any() ?? false;

            wf.DateOfSymptomsOnset = args.DateOfSymptomsOnset;
            wf.LabConfirmationId = null;
            wf.PollToken = _PollTokenService.GenerateToken();

            _Logger.LogDebug($"Committing.");
            _DbContextProvider.SaveAndCommit();

            _Logger.LogInformation($"Committed - new PollToken:{wf.PollToken}.");
            return new AuthorisationResponse {Valid = true, PollToken = wf.PollToken};
        }
    }
}