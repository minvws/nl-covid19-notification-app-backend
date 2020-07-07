// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DevOps
{
    public class CreateWorkflowDatabase
    {
        private readonly WorkflowDbContext _Provider;

        public CreateWorkflowDatabase(IConfiguration configuration)
        {
            var config = new StandardEfDbConfig(configuration, "Workflow");
            var builder = new SqlServerDbContextOptionsBuilder(config);
            _Provider = new WorkflowDbContext(builder.Build());
        }

        public async Task Execute()
        {
            await _Provider.Database.EnsureDeletedAsync();
            await _Provider.Database.EnsureCreatedAsync();
        }

        public async Task AddExampleContent()
        {
            await using var tx = await _Provider.Database.BeginTransactionAsync();

            var wfs1 = new KeyReleaseWorkflowState
            {
                LabConfirmationId = "2L2587",
                BucketId = "2",
                ConfirmationKey = "3",
                Created = new DateTime(2020, 5, 1),
            };

            var key1 = new TemporaryExposureKeyEntity
            {
                Owner = wfs1,
                PublishingState = PublishingState.Unpublished,
                RollingPeriod = 1,
                RollingStartNumber = 1,
                TransmissionRiskLevel = 0,
                KeyData = new byte[0],
                Region = "NL"
            };

            var key2 = new TemporaryExposureKeyEntity
            {
                Owner = wfs1,
                PublishingState = PublishingState.Unpublished,
                RollingPeriod = 1,
                RollingStartNumber = 1,
                TransmissionRiskLevel = 0,
                KeyData = new byte[0],
                Region = "NL"
            };

            await _Provider.KeyReleaseWorkflowStates.AddAsync(wfs1);
            await _Provider.TemporaryExposureKeys.AddRangeAsync(key1, key2);
            _Provider.SaveAndCommit();
        }
    }
}
