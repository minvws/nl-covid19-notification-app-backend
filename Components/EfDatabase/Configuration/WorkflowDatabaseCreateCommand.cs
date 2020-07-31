// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration
{
    public class WorkflowDatabaseCreateCommand
    {
        private readonly WorkflowDbContext _Provider;

        public WorkflowDatabaseCreateCommand(IConfiguration configuration)
        {
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
            var r = new Random();

            var r2 = new StandardRandomNumberGenerator();

            await using var tx = await _Provider.Database.BeginTransactionAsync();

            var wfs1 = new TekReleaseWorkflowStateEntity
            {
                LabConfirmationId = "2L2587",
                BucketId = r2.GenerateKey(),
                ConfirmationKey = r2.GenerateKey(),
                Created = new DateTime(2020, 5, 1),
            };

            var key1 = new TekEntity
            {
                Owner = wfs1,
                PublishingState = PublishingState.Unpublished,
                RollingPeriod = 1,
                RollingStartNumber = 1,
                Region = "NL"
            };
            r.NextBytes(key1.KeyData);

            var key2 = new TekEntity
            {
                Owner = wfs1,
                PublishingState = PublishingState.Unpublished,
                RollingPeriod = 1,
                RollingStartNumber = 1,
                Region = "NL"
            };
            r.NextBytes(key2.KeyData);

            await _Provider.KeyReleaseWorkflowStates.AddAsync(wfs1);
            await _Provider.TemporaryExposureKeys.AddRangeAsync(key1, key2);
            _Provider.SaveAndCommit();
        }
    }
}
