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
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret
{
    public class TekReleaseWorkflowStateCreateV2 : ISecretWriter
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IRandomNumberGenerator _numberGenerator;
        private readonly ILuhnModNGenerator _luhnModNGenerator;
        private readonly ILuhnModNConfig _luhnModNConfig;
        private readonly IWorkflowTime _workflowTime;
        private readonly RegisterSecretLoggingExtensionsV2 _logger;

        private const int AttemptCountMax = 10;
        private int _AttemptCount;

        public TekReleaseWorkflowStateCreateV2(
            WorkflowDbContext dbContextProvider,
            IUtcDateTimeProvider dateTimeProvider,
            IRandomNumberGenerator numberGenerator,
            IWorkflowTime workflowTime,
            RegisterSecretLoggingExtensionsV2 logger,
            ILuhnModNConfig luhnModNConfig,
            ILuhnModNGenerator luhnModNGenerator)
        {
            _workflowDbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _numberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _workflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _luhnModNConfig = luhnModNConfig ?? throw new ArgumentNullException(nameof(luhnModNConfig));
            _luhnModNGenerator = luhnModNGenerator ?? throw new ArgumentNullException(nameof(luhnModNGenerator));
        }

        public async Task<TekReleaseWorkflowStateEntity> ExecuteAsync()
        {
            var entity = await BuildEntityAndAddToContextAsync();

            _logger.WriteWritingStart();

            var success = TryGenerateRemainingFieldsAndWriteToDb(entity);
            while (!success)
            {
                _workflowDbContext.BeginTransaction();
                entity = await BuildEntityAndAddToContextAsync();
                success = TryGenerateRemainingFieldsAndWriteToDb(entity);
            }

            return entity;
        }

        private bool TryGenerateRemainingFieldsAndWriteToDb(TekReleaseWorkflowStateEntity item)
        {
            if (++_AttemptCount > AttemptCountMax)
            {
                _logger.WriteMaximumCreateAttemptsReached();
                throw new InvalidOperationException("Maximum create attempts reached.");
            }

            if (_AttemptCount > 1)
            {
                _logger.WriteDuplicatesFound(_AttemptCount);
            }

            item.GGDKey = _luhnModNGenerator.Next(_luhnModNConfig.ValueLength);
            item.BucketId = _numberGenerator.NextByteArray(UniversalConstants.BucketIdByteCount);
            item.ConfirmationKey = _numberGenerator.NextByteArray(UniversalConstants.ConfirmationKeyByteCount);

            try
            {
                _workflowDbContext.SaveAndCommit();
                _logger.WriteCommitted();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _workflowDbContext.Database.CurrentTransaction.RollbackAsync();
                _workflowDbContext.Remove(item);

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
                return false;

            var errors = new SqlError[sqlEx.Errors.Count];
            sqlEx.Errors.CopyTo(errors, 0);

            return errors.Any(x =>
                x.Number == 2601
                && x.Message.Contains("TekReleaseWorkflowState")
                && (x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.GGDKey))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.ConfirmationKey))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.BucketId)))
            );

        }

        private async Task<TekReleaseWorkflowStateEntity> BuildEntityAndAddToContextAsync()
        {
            var entity = new TekReleaseWorkflowStateEntity
            {
                Created = _dateTimeProvider.Snapshot.Date,
                ValidUntil = _workflowTime.Expiry(_dateTimeProvider.Snapshot)
            };

            await _workflowDbContext.KeyReleaseWorkflowStates.AddAsync(entity);

            return entity;
        }
    }
}
