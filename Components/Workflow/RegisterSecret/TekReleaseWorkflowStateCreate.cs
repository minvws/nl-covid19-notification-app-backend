// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{

    public class TekReleaseWorkflowExpiryTimeProvider : IWorkflowStuff
    {
        private readonly IWorkflowConfig _WorkflowConfig;

        public TekReleaseWorkflowExpiryTimeProvider(IWorkflowConfig workflowConfig)
        {
            _WorkflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
        }

        public DateTime Expiry(DateTime utcNow)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Must be UTC.");

            var result = utcNow.ToLocalTime().Date + TimeSpan.FromMinutes(_WorkflowConfig.TimeToLiveMinutes + _WorkflowConfig.PermittedMobileDeviceClockErrorMinutes);
            return result.ToUniversalTime();
        }

        /// <summary>
        /// Aka Validity
        /// </summary>
        public long TimeToLiveSeconds(DateTime utcNow, DateTime utcExpiry)
        {
            if (utcNow.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Must be UTC.", nameof(utcNow));

            if (utcExpiry.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Must be UTC.", nameof(utcExpiry));

            if (utcNow > utcExpiry)
                throw new ArgumentException("utcNow > utcExpiry.");

            return Convert.ToInt64(Math.Floor((utcExpiry - utcNow).TotalSeconds - _WorkflowConfig.PermittedMobileDeviceClockErrorMinutes));
        }
    }

    public interface IWorkflowStuff
    {
        DateTime Expiry(DateTime utcNow);
        long TimeToLiveSeconds(DateTime utcNow, DateTime utcExpiry);
    }

    public class TekReleaseWorkflowStateCreate : ISecretWriter
    {
        private readonly WorkflowDbContext _DbContextProvider;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IRandomNumberGenerator _NumberGenerator;
        private readonly IWorkflowStuff _WorkflowStuff;
        private readonly ILogger _Logger;
        private int _AttemptCount;

        public TekReleaseWorkflowStateCreate(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, IRandomNumberGenerator numberGenerator, IWorkflowStuff workflowStuff, ILogger<TekReleaseWorkflowStateCreate> logger)
        {
            _DbContextProvider = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _NumberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _WorkflowStuff = workflowStuff ?? throw new ArgumentNullException(nameof(workflowStuff));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TekReleaseWorkflowStateEntity> Execute()
        {
            var snapshot = _DateTimeProvider.Now();
            var entity = new TekReleaseWorkflowStateEntity
            {
                Created = snapshot,
                ValidUntil = _WorkflowStuff.Expiry(snapshot)
            };
            await _DbContextProvider.KeyReleaseWorkflowStates.AddAsync(entity);

            _Logger.LogDebug("Writing.");

            var success = WriteAttempt(entity);
            while (!success)
                success = WriteAttempt(entity);


            return entity;
        }

        private const int AttemptCountMax = 5;

        private bool WriteAttempt(TekReleaseWorkflowStateEntity item)
        {
            if (++_AttemptCount > AttemptCountMax)
                throw new InvalidOperationException("Maximum create attempts reached.");

            if (_AttemptCount > 1)
                _Logger.LogWarning($"Duplicates found while creating workflow - attempt:{_AttemptCount}");

            item.LabConfirmationId = _NumberGenerator.GenerateToken();
            item.BucketId = _NumberGenerator.GenerateKey();
            item.ConfirmationKey = _NumberGenerator.GenerateKey();

            try
            {
                _DbContextProvider.SaveAndCommit();
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

        private bool CanRetry(SqlException ex)
        {
            if (ex.Number == 2627)
            {
                //TODO check actual violated index is one of the 3

                return true;
            }

            return false;
        }

#if DEBUG
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
                    && x.Message.Contains($"TekReleaseWorkflowState")
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
#endif
    }
}