// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.RegisterSecret;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.GenerateTeks.Commands
{
    public class GenerateTeksCommand
    {
        private readonly WorkflowDbContext _workflowDb;
        private readonly IRandomNumberGenerator _numberGenerator;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ILuhnModNGenerator _luhnModNGenerator;
        private readonly ILuhnModNConfig _luhnModNConfig;
        private readonly IWorkflowTime _workflowTime;
        private readonly ILogger _logger;

        private GenerateTeksCommandArgs _args;

        public GenerateTeksCommand(
            WorkflowDbContext workflowDb,
            IRandomNumberGenerator numberGenerator,
            IUtcDateTimeProvider dateTimeProvider,
            IWorkflowTime workflowTime,
            ILuhnModNConfig luhnModNConfig,
            ILuhnModNGenerator luhnModNGenerator,
            ILogger<GenerateTeksCommand> logger)
        {
            _workflowDb = workflowDb ?? throw new ArgumentNullException(nameof(workflowDb));
            _numberGenerator = numberGenerator ?? throw new ArgumentNullException(nameof(numberGenerator));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _workflowTime = workflowTime ?? throw new ArgumentNullException(nameof(workflowTime));
            _luhnModNConfig = luhnModNConfig ?? throw new ArgumentNullException(nameof(luhnModNConfig));
            _luhnModNGenerator = luhnModNGenerator ?? throw new ArgumentNullException(nameof(luhnModNGenerator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(GenerateTeksCommandArgs args)
        {
            _args = args;
            await GenWorkflowsAsync();
        }

        private async Task GenWorkflowsAsync()
        {
            await _workflowDb.BulkDeleteAsync(_workflowDb.KeyReleaseWorkflowStates.ToList());

            var workflowList = new List<TekReleaseWorkflowStateEntity>();
            var tekList = new List<TekEntity>();

            _logger.LogDebug("Writing TekReleaseWorkflowStateEntities");
            for (var i = 0; i < _args.WorkflowCount; i++)
            {
                var workflowStateEntity = new TekReleaseWorkflowStateEntity
                {
                    Created = _dateTimeProvider.Snapshot.Date,
                    ValidUntil = _workflowTime.Expiry(_dateTimeProvider.Snapshot),
                    GGDKey = GenerateUniqueGGDKey(workflowList),
                    BucketId = GenerateUniqueBucketId(workflowList),
                    ConfirmationKey = GenerateUniqueConfirmationKey(workflowList),
                    AuthorisedByCaregiver = DateTime.UtcNow,
                    StartDateOfTekInclusion = DateTime.UtcNow.AddDays(_numberGenerator.Next(-13, 0)),
                    IsSymptomatic = (InfectiousPeriodType)_numberGenerator.Next(0, 1),
                    IsOriginPortal = Convert.ToBoolean(_numberGenerator.Next(0, 1))
                };

                _logger.LogDebug("Adding workflowStateEntity # {WorkflowStateEntityCount}", i);

                workflowList.Add(workflowStateEntity);
            }

            var bulkConfig = new BulkConfig { SetOutputIdentity = true, BatchSize = 100 };

            _logger.LogDebug("BeginTransactionAsync:");
            await using var transaction = await _workflowDb.Database.BeginTransactionAsync();

            _logger.LogDebug("BulkInsertAsync [workflowList]");
            await _workflowDb.BulkInsertAsync(workflowList, bulkConfig);

            foreach (var entity in workflowList)
            {
                _logger.LogDebug("Generate Teks for TekReleaseWorkflowStateEntity");
                GenTeks(entity);

                foreach (var tek in entity.Teks)
                {
                    tek.OwnerId = entity.Id; // setting FK to match its linked PK that was generated in DB
                }
                tekList.AddRange(entity.Teks);
            }

            _logger.LogDebug("BulkInsertAsync [tekList]");
            await _workflowDb.BulkInsertAsync(tekList);

            _logger.LogDebug("CommitAsync");
            await transaction.CommitAsync();
        }

        private string GenerateUniqueGGDKey(List<TekReleaseWorkflowStateEntity> workflowList)
        {
            var ggdKey = _luhnModNGenerator.Next(_luhnModNConfig.ValueLength);

            if (workflowList.Any(p => p.GGDKey == ggdKey))
            {
                return GenerateUniqueGGDKey(workflowList);
            }

            return ggdKey;
        }

        private byte[] GenerateUniqueBucketId(List<TekReleaseWorkflowStateEntity> workflowList)
        {
            var bucketId = _numberGenerator.NextByteArray(UniversalConstants.BucketIdByteCount);

            if (workflowList.Any(p => p.BucketId == bucketId))
            {
                return GenerateUniqueBucketId(workflowList);
            }

            return bucketId;
        }

        private byte[] GenerateUniqueConfirmationKey(List<TekReleaseWorkflowStateEntity> workflowList)
        {
            var confirmationKey = _numberGenerator.NextByteArray(UniversalConstants.BucketIdByteCount);

            if (workflowList.Any(p => p.ConfirmationKey == confirmationKey))
            {
                return GenerateUniqueConfirmationKey(workflowList);
            }

            return confirmationKey;
        }

        private void GenTeks(TekReleaseWorkflowStateEntity owner)
        {
            for (var i = 0; i < _args.TekCountPerWorkflow; i++)
            {
                var k = new TekEntity
                {
                    Owner = owner,
                    PublishingState = PublishingState.Unpublished,
                    RollingStartNumber = DateTime.UtcNow.Date.ToRollingStartNumber(),
                    RollingPeriod = _numberGenerator.Next(1, UniversalConstants.RollingPeriodRange.Hi),
                    KeyData = _numberGenerator.NextByteArray(UniversalConstants.DailyKeyDataByteCount),
                    PublishAfter = DateTime.UtcNow,
                };
                owner.Teks.Add(k);
            }
        }
    }
}
