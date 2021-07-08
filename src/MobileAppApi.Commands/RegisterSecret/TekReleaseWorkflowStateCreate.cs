// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class TekReleaseWorkflowStateCreate : ISecretWriter
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IRandomNumberGenerator _numberGenerator;
        private readonly ILabConfirmationIdService _labConfirmationIdService;
        private readonly IWorkflowTime _workflowTime;
        private readonly RegisterSecretLoggingExtensions _logger;

        private const int AttemptCountMax = 10;
        private int _attemptCount;

        public TekReleaseWorkflowStateCreate(
            WorkflowDbContext dbContextProvider,
            IUtcDateTimeProvider dateTimeProvider,
            IRandomNumberGenerator numberGenerator,
            ILabConfirmationIdService labConfirmationIdService,
            IWorkflowTime workflowTime,
            RegisterSecretLoggingExtensions logger)
        {
            _workflowDbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _numberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _labConfirmationIdService = labConfirmationIdService ?? throw new ArgumentNullException(nameof(labConfirmationIdService));
            _workflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TekReleaseWorkflowStateEntity> ExecuteAsync()
        {
            var entity = new TekReleaseWorkflowStateEntity
            {
                Created = _dateTimeProvider.Snapshot.Date,
                ValidUntil = _workflowTime.Expiry(_dateTimeProvider.Snapshot),
                LabConfirmationId = _labConfirmationIdService.Next(),
                BucketId = _numberGenerator.NextByteArray(UniversalConstants.BucketIdByteCount),
                ConfirmationKey = _numberGenerator.NextByteArray(UniversalConstants.ConfirmationKeyByteCount)
            };

            _logger.WriteWritingStart();

            var success = await BuildEntityAndAddToContextAsync(entity);
            while (!success)
            {
                success = await BuildEntityAndAddToContextAsync(entity);
            }

            return entity;
        }

        private async Task<bool> BuildEntityAndAddToContextAsync(TekReleaseWorkflowStateEntity entity)
        {
            if (++_attemptCount > AttemptCountMax)
            {
                _logger.WriteMaximumCreateAttemptsReached();
                throw new InvalidOperationException("Maximum create attempts reached.");
            }

            if (_attemptCount > 1)
            {
                _logger.WriteDuplicatesFound(_attemptCount);
            }

            _workflowDbContext.BeginTransaction();

            try
            {
                await _workflowDbContext.KeyReleaseWorkflowStates.AddAsync(entity);

                _workflowDbContext.SaveAndCommit();
                _logger.WriteCommitted();
                return true;
            }
            catch (DbUpdateException ex)
            {
                await _workflowDbContext.Database.CurrentTransaction.RollbackAsync();
                _workflowDbContext.KeyReleaseWorkflowStates.Remove(entity);

                if (CanRetry(ex))
                {
                    return false;
                }
                return false;
                //throw;
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
                && (x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.LabConfirmationId))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.ConfirmationKey))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.BucketId)))
            );
        }
    }
}
