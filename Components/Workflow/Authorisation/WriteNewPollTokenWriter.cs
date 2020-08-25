// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation
{
    public class WriteNewPollTokenWriter
    {
        private readonly WorkflowDbContext _WorkflowDb;
        private readonly PollTokenService _PollTokenService;
        private readonly ILogger _Logger;
        private int _AttemptCount;
        private const int AttemptCountMax = 5;

        public WriteNewPollTokenWriter(WorkflowDbContext workflowDb, PollTokenService pollTokenService, ILogger<WriteNewPollTokenWriter> logger)
        {
            _WorkflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _PollTokenService = pollTokenService ?? throw new ArgumentNullException(nameof(pollTokenService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Execute(TekReleaseWorkflowStateEntity wf)
        {
            _Logger.LogDebug("Writing.");

            var success = WriteAttempt(wf);
            while (!success)
                success = WriteAttempt(wf);

            return wf.PollToken;
        }

        private bool WriteAttempt(TekReleaseWorkflowStateEntity wf)
        {
            if (++_AttemptCount > AttemptCountMax)
                throw new InvalidOperationException("Maximum attempts reached.");

            if (_AttemptCount > 1)
                _Logger.LogWarning("Duplicate PollToken found - attempt:{AttemptCount}", _AttemptCount);

            wf.PollToken = _PollTokenService.Next();

            try
            {
                _WorkflowDb.SaveAndCommit();
                _Logger.LogDebug("Committed.");
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (CanRetry(ex))
                    return false;

                throw;
            }
        }

        private bool CanRetry(DbUpdateException ex)
        {
            //E.g. Microsoft.Data.SqlClient.SqlException(0x80131904):
            //Cannot insert duplicate key row in object 'dbo.TekReleaseWorkflowState'
            //with unique index 'IX_TekReleaseWorkflowState_BucketId'.The duplicate key value is (blah blah).
            if (ex.InnerException is SqlException sqlEx)
            {
                var errors = new SqlError[sqlEx.Errors.Count];
                sqlEx.Errors.CopyTo(errors, 0);

                return errors.Any(x =>
                    x.Number == 2601
                    && x.Message.Contains("TekReleaseWorkflowState")
                    && x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.PollToken)));
            }

            //e.g. SQLite Error 19: 'UNIQUE constraint failed: TekReleaseWorkflowState.LabConfirmationId'.
            return ex.InnerException is SqliteException inner
                   && inner.SqliteErrorCode == 19
                   && inner.Message.Contains($"TekReleaseWorkflowState.{nameof(TekReleaseWorkflowStateEntity.PollToken)}");
        }
    }
}