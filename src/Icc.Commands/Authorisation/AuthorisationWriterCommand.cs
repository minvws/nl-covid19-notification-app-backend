// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class AuthorisationWriterCommand
    {
        private readonly WorkflowDbContext _WorkflowDb;
        private readonly ILogger _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly WriteNewPollTokenWriter _NewPollTokenWriter;

        public AuthorisationWriterCommand(WorkflowDbContext workflowDb, ILogger<AuthorisationWriterCommand> logger, IUtcDateTimeProvider dateTimeProvider, WriteNewPollTokenWriter newPollTokenWriter)
        {
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _NewPollTokenWriter = newPollTokenWriter ?? throw new ArgumentNullException(nameof(newPollTokenWriter));
        }

        /// <summary>
        /// Assume args validated
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public async Task<string> ExecuteAsync(AuthorisationArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            var wf = await _WorkflowDb
                .KeyReleaseWorkflowStates
                .Include(x => x.Teks)
                .FirstOrDefaultAsync(x => x.LabConfirmationId == args.LabConfirmationId);

            if (wf == null)
            {
                _Logger.WriteKeyReleaseWorkflowStateNotFound(args.LabConfirmationId);
                return null;
            }

            _Logger.LogInformation("LabConfirmationId {LabConfirmationId} authorized.", wf.LabConfirmationId);

            wf.AuthorisedByCaregiver = _DateTimeProvider.Snapshot;
            wf.LabConfirmationId = null; //Clear from usable key range
            wf.StartDateOfTekInclusion = args.DateOfSymptomsOnset;

            return _NewPollTokenWriter.Execute(wf);
        }
    }
}
