// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public class WorkflowDatabaseCreateCommand
    {
        private readonly WorkflowDbContext _Provider;

        private readonly IWorkflowConfig _WorkflowConfig;
        private readonly ITekValidatorConfig _TekValidatorConfig;
        private readonly ILabConfirmationIdService _LabConfirmationIdService;

        public WorkflowDatabaseCreateCommand(IConfiguration configuration, ITekValidatorConfig tekValidatorConfig, ILabConfirmationIdService labConfirmationIdService, IWorkflowConfig workflowConfig)
        {
            _TekValidatorConfig = tekValidatorConfig ?? throw new ArgumentNullException(nameof(tekValidatorConfig));
            _LabConfirmationIdService = labConfirmationIdService ?? throw new ArgumentNullException(nameof(labConfirmationIdService));
            _WorkflowConfig = workflowConfig ?? throw new ArgumentNullException(nameof(workflowConfig));
            var config = new StandardEfDbConfig(configuration, "Workflow");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _Provider = new WorkflowDbContext(builder.Build());
        }

        public async Task Execute(bool nuke)
        {
            if (nuke) await _Provider.Database.EnsureDeletedAsync();
            await _Provider.Database.EnsureCreatedAsync();
        }

        public async Task AddExampleContent()
        {
            var r2 = new StandardRandomNumberGenerator();

            await using var tx = await _Provider.Database.BeginTransactionAsync();

            var wfs1 = new TekReleaseWorkflowStateEntity
            {
                LabConfirmationId = _LabConfirmationIdService.Next(),
                BucketId = r2.NextByteArray(_WorkflowConfig.BucketIdLength),
                ConfirmationKey = r2.NextByteArray(_WorkflowConfig.ConfirmationKeyLength),
                Created = new DateTime(2020, 5, 1),
            };

            var key1 = new TekEntity
            {
                Owner = wfs1,
                PublishingState = PublishingState.Unpublished,
                RollingPeriod = 2,
                RollingStartNumber = DateTime.Now.ToRollingStartNumber(),
                KeyData = r2.NextByteArray(_TekValidatorConfig.KeyDataLength),
                Region = "NL"
            };


            var key2 = new TekEntity
            {
                Owner = wfs1,
                PublishingState = PublishingState.Unpublished,
                RollingPeriod = 144,
                RollingStartNumber = DateTime.Now.ToRollingStartNumber(),
                KeyData = r2.NextByteArray(_TekValidatorConfig.KeyDataLength),
                Region = "NL"
            };

            await _Provider.KeyReleaseWorkflowStates.AddAsync(wfs1);
            await _Provider.TemporaryExposureKeys.AddRangeAsync(key1, key2);
            _Provider.SaveAndCommit();
        }
    }
}
