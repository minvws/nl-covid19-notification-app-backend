// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public class TekReleaseWorkflowStateCreate : ISecretWriter
    {
        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IRandomNumberGenerator _NumberGenerator;
        private readonly ILabConfirmationIdService _LabConfirmationIdService;
        private readonly IWorkflowTime _WorkflowTime;
        private readonly IWorkflowConfig _WorkflowConfig;
        private readonly ILogger _Logger;

        private const int AttemptCountMax = 10;
        private int _AttemptCount;

        public TekReleaseWorkflowStateCreate(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, IRandomNumberGenerator numberGenerator, ILabConfirmationIdService labConfirmationIdService, IWorkflowTime workflowTime, IWorkflowConfig workflowConfig, ILogger<TekReleaseWorkflowStateCreate> logger)
        {
            _WorkflowDbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _NumberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _LabConfirmationIdService = labConfirmationIdService ?? throw new ArgumentNullException(nameof(labConfirmationIdService));
            _WorkflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _WorkflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TekReleaseWorkflowStateEntity> Execute()
        {
            var entity = new TekReleaseWorkflowStateEntity
            {
                Created = _DateTimeProvider.Snapshot,
                ValidUntil = _WorkflowTime.Expiry(_DateTimeProvider.Snapshot)
            };
            await _WorkflowDbContext.KeyReleaseWorkflowStates.AddAsync(entity);

            _Logger.LogDebug("Writing.");

            var success = WriteAttempt(entity);
            while (!success)
            {
                entity = new TekReleaseWorkflowStateEntity
                {
                    Created = _DateTimeProvider.Snapshot,
                    ValidUntil = _WorkflowTime.Expiry(_DateTimeProvider.Snapshot)
                };
                await _WorkflowDbContext.KeyReleaseWorkflowStates.AddAsync(entity);
                success = WriteAttempt(entity);
            }

            return entity;
        }

        private bool WriteAttempt(TekReleaseWorkflowStateEntity item)
        {
            if (++_AttemptCount > AttemptCountMax)
                throw new InvalidOperationException("Maximum create attempts reached.");

            if (_AttemptCount > 1)
                _Logger.LogWarning("Duplicates found while creating workflow - attempt:{AttemptCount}", _AttemptCount);

            item.LabConfirmationId = _LabConfirmationIdService.Next();
            item.BucketId = _NumberGenerator.NextByteArray(_WorkflowConfig.BucketIdLength);
            item.ConfirmationKey = _NumberGenerator.NextByteArray(_WorkflowConfig.ConfirmationKeyLength);

            try
            {
                _WorkflowDbContext.SaveAndCommit();
                _Logger.LogDebug("Committed.");
                return true;
            }
            catch (DbUpdateException ex)
            {
                _WorkflowDbContext.Remove(item);
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
                    && (x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.LabConfirmationId))
                        || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.ConfirmationKey))
                        || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.BucketId)))
                );
            }

            //e.g. SQLite Error 19: 'UNIQUE constraint failed: TekReleaseWorkflowState.LabConfirmationId'.
            return ex.InnerException is SqliteException inner 
                   && inner.SqliteErrorCode == 19
                   && (
                      inner.Message.Contains($"TekReleaseWorkflowState.{nameof(TekReleaseWorkflowStateEntity.LabConfirmationId)}")
                      || inner.Message.Contains($"TekReleaseWorkflowState.{nameof(TekReleaseWorkflowStateEntity.ConfirmationKey)}")
                      || inner.Message.Contains($"TekReleaseWorkflowState.{nameof(TekReleaseWorkflowStateEntity.BucketId)}")
                  );
        }
   }
}