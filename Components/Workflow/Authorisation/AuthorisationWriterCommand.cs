// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JWT;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class AuthorisationWriterCommand
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly PollTokenService _PollTokenService;
        private readonly ILogger _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
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
                .Include(x => x.Teks)
                .FirstOrDefaultAsync(x => x.LabConfirmationId == args.LabConfirmationId);

            if (wf == null)
            {
                _Logger.LogError("KeyReleaseWorkflowState not found - LabConfirmationId:{LabConfirmationId}.", args.LabConfirmationId);
                return new AuthorisationResponse {Valid = false};
            }

            wf.AuthorisedByCaregiver = _DateTimeProvider.Now();
            wf.LabConfirmationId = null; //Clear from usable key range
            wf.DateOfSymptomsOnset = args.DateOfSymptomsOnset;
            wf.PollToken = _PollTokenService.GenerateToken();

            _Logger.LogDebug("Committing.");
            _DbContextProvider.SaveAndCommit();

            _Logger.LogInformation("Committed - new PollToken:{PollToken}.", wf.PollToken);
            return new AuthorisationResponse {Valid = true, PollToken = wf.PollToken};
        }
    }
}