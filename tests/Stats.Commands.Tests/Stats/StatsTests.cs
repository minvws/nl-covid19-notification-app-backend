// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands.Tests.Stats
{
    public abstract class StatsTests
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly StatsDbContext _statsDbContext;
        private readonly IUtcDateTimeProvider _fakeDtp;

        protected StatsTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions, DbContextOptions<StatsDbContext> statsDbContextOptions)
        {
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions ?? throw new ArgumentNullException(nameof(workflowDbContextOptions)));
            _workflowDbContext.Database.EnsureCreated();
            _statsDbContext = new StatsDbContext(statsDbContextOptions ?? throw new ArgumentNullException(nameof(statsDbContextOptions)));
            _statsDbContext.Database.EnsureCreated();

            var snapshot = DateTime.UtcNow;
            var mock = new Mock<IUtcDateTimeProvider>(MockBehavior.Strict);
            mock.Setup(x => x.Snapshot).Returns(snapshot);
            _fakeDtp = mock.Object;
        }

        [Fact]
        public async Task NothingToSeeHere()
        {
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            await _statsDbContext.BulkDeleteAsync(_statsDbContext.StatisticsEntries.ToList());

            var cmd = new StatisticsCommand(new StatisticsDbWriter(_statsDbContext, _fakeDtp),
                new IStatsQueryCommand[]
                {
                    new TotalWorkflowCountStatsQueryCommand(_workflowDbContext),
                    new TotalWorkflowsWithTeksQueryCommand(_workflowDbContext),
                    new TotalWorkflowAuthorisedCountStatsQueryCommand(_workflowDbContext),
                    new PublishedTekCountStatsQueryCommand(_workflowDbContext),
                    new TotalTekCountStatsQueryCommand(_workflowDbContext),
                });
            await cmd.ExecuteAsync();

            Assert.Equal(0, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalWorkflowCountStatsQueryCommand.Name).Value);
            Assert.Equal(0, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalWorkflowsWithTeksQueryCommand.Name).Value);
            Assert.Equal(0, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalWorkflowAuthorisedCountStatsQueryCommand.Name).Value);
            Assert.Equal(0, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == PublishedTekCountStatsQueryCommand.Name).Value);
            Assert.Equal(0, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalTekCountStatsQueryCommand.Name).Value);
        }

        [Fact]
        public async Task WithData()
        {
            await _workflowDbContext.BulkDeleteAsync(_workflowDbContext.KeyReleaseWorkflowStates.ToList());
            await _statsDbContext.BulkDeleteAsync(_statsDbContext.StatisticsEntries.ToList());

            var workflows = new[] {
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{1}, ConfirmationKey = new byte[]{1}, LabConfirmationId = "1", Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>()},
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{2}, ConfirmationKey = new byte[]{2}, LabConfirmationId = "2",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{3}, ConfirmationKey = new byte[]{3}, LabConfirmationId = "3",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{4}, ConfirmationKey = new byte[]{4}, LabConfirmationId = "4",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{5}, ConfirmationKey = new byte[]{5}, LabConfirmationId = "5",  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{6}, ConfirmationKey = new byte[]{6}, LabConfirmationId = null,  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{7}, ConfirmationKey = new byte[]{7}, LabConfirmationId = null,  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() },
                new TekReleaseWorkflowStateEntity { BucketId = new byte[]{8}, ConfirmationKey = new byte[]{8}, LabConfirmationId = null,  Created = DateTime.MinValue, ValidUntil = DateTime.MinValue, Teks = new List<TekEntity>() }
            };

            ((List<TekEntity>)workflows[0].Teks).AddRange(new[] { new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue } });


            ((List<TekEntity>)workflows[4].Teks).AddRange(new[]
            { new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue },
            });

            ((List<TekEntity>)workflows[5].Teks).AddRange(new[]
            { new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue },
                new TekEntity { KeyData = new byte[0], PublishingState = PublishingState.Unpublished, PublishAfter = DateTime.MinValue }
            });

            await _workflowDbContext.KeyReleaseWorkflowStates.AddRangeAsync(workflows);
            await _workflowDbContext.SaveChangesAsync();

            var cmd = new StatisticsCommand(new StatisticsDbWriter(_statsDbContext, _fakeDtp),
                new IStatsQueryCommand[]
                {
                    new TotalWorkflowCountStatsQueryCommand(_workflowDbContext),
                    new TotalWorkflowsWithTeksQueryCommand(_workflowDbContext),
                    new TotalWorkflowAuthorisedCountStatsQueryCommand(_workflowDbContext),
                    new PublishedTekCountStatsQueryCommand(_workflowDbContext),
                    new TotalTekCountStatsQueryCommand(_workflowDbContext),
                });
            await cmd.ExecuteAsync();

            Assert.Equal(8, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalWorkflowCountStatsQueryCommand.Name).Value);
            Assert.Equal(3, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalWorkflowsWithTeksQueryCommand.Name).Value);
            Assert.Equal(3, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalWorkflowAuthorisedCountStatsQueryCommand.Name).Value);
            Assert.Equal(4, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == PublishedTekCountStatsQueryCommand.Name).Value);
            Assert.Equal(9, _statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _fakeDtp.Snapshot.Date && x.Name == TotalTekCountStatsQueryCommand.Name).Value);
        }
    }
}
