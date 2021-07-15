// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
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

        public TekReleaseWorkflowStateCreateV2(
            WorkflowDbContext workflowDbContext,
            IUtcDateTimeProvider dateTimeProvider,
            IRandomNumberGenerator numberGenerator,
            IWorkflowTime workflowTime,
            ILuhnModNConfig luhnModNConfig,
            ILuhnModNGenerator luhnModNGenerator,
            RegisterSecretLoggingExtensionsV2 logger)
        {
            _workflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _numberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _workflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _luhnModNConfig = luhnModNConfig ?? throw new ArgumentNullException(nameof(luhnModNConfig));
            _luhnModNGenerator = luhnModNGenerator ?? throw new ArgumentNullException(nameof(luhnModNGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private const int AttemptCountMax = 10;
        private int _attemptCount;

        public async Task<TekReleaseWorkflowStateEntity> ExecuteAsync()
        {
            // Create entity with only Created and ValidUntil dates.
            var entity = new TekReleaseWorkflowStateEntity
            {
                Created = _dateTimeProvider.Snapshot.Date,
                ValidUntil = _workflowTime.Expiry(_dateTimeProvider.Snapshot)
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
                // Generate GGDKey, BucketId and ConfirmationKey. When the commit result in an collision caused by a duplicated value of GGDKey and BucketId these will be generated in a second attempt.
                entity.GGDKey = GenerateUniqueGGDKey(); // Generate GGDKey including peek in database
                entity.BucketId = _numberGenerator.NextByteArray(UniversalConstants.BucketIdByteCount);
                entity.ConfirmationKey = _numberGenerator.NextByteArray(UniversalConstants.ConfirmationKeyByteCount);

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
            }
        }

        private string GenerateUniqueGGDKey()
        {
            var ggdKey = _luhnModNGenerator.Next(_luhnModNConfig.ValueLength);

            if (_workflowDbContext.KeyReleaseWorkflowStates.AsNoTracking().Any(p => p.GGDKey == ggdKey))
            {
                return GenerateUniqueGGDKey();
            }

            return ggdKey;
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
                && (x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.GGDKey))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.ConfirmationKey))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.BucketId)))
            );
        }
    }
}
