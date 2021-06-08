// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation
{
    public class WriteNewPollTokenWriter
    {
        private readonly WorkflowDbContext _workflowDb;
        private readonly IPollTokenService _pollTokenService;
        private readonly ILogger _logger;
        private int _attemptCount;
        private const int AttemptCountMax = 5;

        public WriteNewPollTokenWriter(WorkflowDbContext workflowDb, IPollTokenService pollTokenService, ILogger<WriteNewPollTokenWriter> logger)
        {
            _workflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _pollTokenService = pollTokenService ?? throw new ArgumentNullException(nameof(pollTokenService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string Execute(TekReleaseWorkflowStateEntity wf)
        {
            _logger.WriteWritingNewPollToken();

            var success = WriteAttempt(wf);
            while (!success)
            {
                success = WriteAttempt(wf);
            }

            return wf.PollToken;
        }

        private bool WriteAttempt(TekReleaseWorkflowStateEntity wf)
        {
            if (++_attemptCount > AttemptCountMax)
            {
                throw new InvalidOperationException("Maximum attempts reached.");
            }

            if (_attemptCount > 1)
            {
                _logger.WriteDuplicatePollTokenFound(_attemptCount);
            }

            wf.PollToken = _pollTokenService.Next();

            try
            {
                _workflowDb.SaveAndCommit();
                _logger.WritePollTokenCommit();
                return true;
            }
            catch (DbUpdateException ex)
            {
                if (CanRetry(ex))
                {
                    return false;
                }

                throw;
            }
        }

        //E.g. Microsoft.Data.SqlClient.SqlException(0x80131904):
        //Cannot insert duplicate key row in object 'dbo.TekReleaseWorkflowState'
        //with unique index 'IX_TekReleaseWorkflowState_BucketId'.The duplicate key value is (blah blah).
        private bool CanRetry(DbUpdateException ex)
        {
            if (!(ex.InnerException is SqlException sqlEx))
            {
                return false;
            }

            var errors = new SqlError[sqlEx.Errors.Count];
            sqlEx.Errors.CopyTo(errors, 0);

            return errors.Any(x =>
                x.Number == 2601
                && x.Message.Contains("TekReleaseWorkflowState")
                && x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.PollToken)));
        }
    }
}