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
        private readonly WorkflowDbContext _WorkflowDbContext;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IRandomNumberGenerator _NumberGenerator;
        private readonly ILabConfirmationIdService _LabConfirmationIdService;
        //private readonly ILuhnModNGenerator _LuhnModNGenerator;
        private readonly IWorkflowTime _WorkflowTime;
        private readonly IWorkflowConfig _WorkflowConfig;
        private readonly RegisterSecretLoggingExtensions _Logger;

        private const int AttemptCountMax = 10;
        private int _AttemptCount;

        public TekReleaseWorkflowStateCreate(WorkflowDbContext dbContextProvider, IUtcDateTimeProvider dateTimeProvider, IRandomNumberGenerator numberGenerator, ILabConfirmationIdService labConfirmationIdService, IWorkflowTime workflowTime, IWorkflowConfig workflowConfig, RegisterSecretLoggingExtensions logger)
        {
            _WorkflowDbContext = dbContextProvider ?? throw new ArgumentNullException(nameof(dbContextProvider));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _NumberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _LabConfirmationIdService = labConfirmationIdService ?? throw new ArgumentNullException(nameof(labConfirmationIdService));
            _WorkflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _WorkflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TekReleaseWorkflowStateEntity> ExecuteAsync()
        {
            var entity = await BuildEntityAsync();

            _Logger.WriteWritingStart();

            var success = WriteAttempt(entity, includeLuhnModNDigit: false);
            while (!success)
            {
                _WorkflowDbContext.BeginTransaction();
                entity = await BuildEntityAsync();
                success = WriteAttempt(entity);
            }

            return entity;
        }

        public async Task<TekReleaseWorkflowStateEntity> ExecuteV2Async()
        {
            var entity = await BuildEntityAsync();

            _Logger.WriteWritingStart();

            var success = WriteAttempt(entity);
            while (!success)
            {
                _WorkflowDbContext.BeginTransaction();
                entity = await BuildEntityAsync();
                success = WriteAttempt(entity);
            }

            return entity;
        }

        private bool WriteAttempt(TekReleaseWorkflowStateEntity item, bool includeLuhnModNDigit = true)
        {
            if (++_AttemptCount > AttemptCountMax)
            {
                _Logger.WriteMaximumCreateAttemptsReached();
                throw new InvalidOperationException("Maximum create attempts reached.");
            }

            if (_AttemptCount > 1)
            {
                _Logger.WriteDuplicatesFound(_AttemptCount);
            }

            if (includeLuhnModNDigit)
            {
                //item.GGDKey = _LuhnModNGenerator.Next();
            }
            else
            {
                item.LabConfirmationId = _LabConfirmationIdService.Next();
            }

            item.BucketId = _NumberGenerator.NextByteArray(UniversalConstants.BucketIdByteCount);
            item.ConfirmationKey = _NumberGenerator.NextByteArray(UniversalConstants.ConfirmationKeyByteCount);

            try
            {
                _WorkflowDbContext.SaveAndCommit();
                _Logger.WriteCommitted();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _WorkflowDbContext.Database.CurrentTransaction.RollbackAsync();
                _WorkflowDbContext.Remove(item);

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
                && (x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.LabConfirmationId))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.ConfirmationKey))
                    || x.Message.Contains(nameof(TekReleaseWorkflowStateEntity.BucketId)))
            );

        }

        private async Task<TekReleaseWorkflowStateEntity> BuildEntityAsync()
        {
            var entity = new TekReleaseWorkflowStateEntity
            {
                Created = _DateTimeProvider.Snapshot.Date,
                ValidUntil = _WorkflowTime.Expiry(_DateTimeProvider.Snapshot)
            };

            await _WorkflowDbContext.KeyReleaseWorkflowStates.AddAsync(entity);

            return entity;
        }
    }
}