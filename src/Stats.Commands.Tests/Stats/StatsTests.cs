// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands.Tests.Stats
{
    public abstract class StatsTests : IDisposable
    {

        private readonly IDbProvider<WorkflowDbContext> _WorkflowDbProvider;
        private readonly IDbProvider<StatsDbContext> _StatsDbProvider;
        private IUtcDateTimeProvider _FakeDtp;

        protected StatsTests(IDbProvider<WorkflowDbContext> workflowDbProvider, IDbProvider<StatsDbContext> statsDbProvider)
        {
            _WorkflowDbProvider = workflowDbProvider ?? throw new ArgumentNullException(nameof(workflowDbProvider));
            _StatsDbProvider = statsDbProvider ?? throw new ArgumentNullException(nameof(statsDbProvider));
            var snapshot = DateTime.UtcNow;
            var mock = new Mock<IUtcDateTimeProvider>(MockBehavior.Strict);
            mock.Setup(x => x.Snapshot).Returns(snapshot);
            _FakeDtp = mock.Object;
        }

        [Fact]
        [ExclusivelyUses(nameof(StatsTests))]
        public void NothingToSeeHere()
        {
            var wf = _WorkflowDbProvider.CreateNew();
            var statsDbContext = _StatsDbProvider.CreateNew();
            var cmd = new StatisticsCommand(new StatisticsDbWriter(statsDbContext, _FakeDtp),
                new IStatsQueryCommand[]
                {
                    new TotalWorkflowCountStatsQueryCommand(wf),
                    new TotalWorkflowsWithTeksQueryCommand(wf),
                    new TotalWorkflowAuthorisedCountStatsQueryCommand(wf),
                    new PublishedTekCountStatsQueryCommand(wf),
                    new TotalTekCountStatsQueryCommand(wf),
                });
                cmd.Execute();

                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowCountStatsQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowsWithTeksQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowAuthorisedCountStatsQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == PublishedTekCountStatsQueryCommand.Name).Value);
                Assert.Equal(0, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalTekCountStatsQueryCommand.Name).Value);
        }

        [Fact]
        [ExclusivelyUses(nameof(StatsTests))]
        public void WithData()
        {
            var wf = _WorkflowDbProvider.CreateNew();

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

            ((List<TekEntity>) workflows[0].Teks).AddRange(new []{new TekEntity{KeyData = new byte[0], PublishingState = PublishingState.Published, PublishAfter = DateTime.MinValue } });


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

            wf.KeyReleaseWorkflowStates.AddRange(workflows);
            wf.SaveChanges();

            var statsDbContext = _StatsDbProvider.CreateNew();
            var cmd = new StatisticsCommand(new StatisticsDbWriter(statsDbContext, _FakeDtp),
                new IStatsQueryCommand[]
                {
                    new TotalWorkflowCountStatsQueryCommand(wf),
                    new TotalWorkflowsWithTeksQueryCommand(wf),
                    new TotalWorkflowAuthorisedCountStatsQueryCommand(wf),
                    new PublishedTekCountStatsQueryCommand(wf),
                    new TotalTekCountStatsQueryCommand(wf),
                });
            cmd.Execute();

            Assert.Equal(8, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowCountStatsQueryCommand.Name).Value);
            Assert.Equal(3, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowsWithTeksQueryCommand.Name).Value);
            Assert.Equal(3, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalWorkflowAuthorisedCountStatsQueryCommand.Name).Value);
            Assert.Equal(4, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == PublishedTekCountStatsQueryCommand.Name).Value);
            Assert.Equal(9, statsDbContext.StatisticsEntries.Single(x => x.Created.Date == _FakeDtp.Snapshot.Date && x.Name == TotalTekCountStatsQueryCommand.Name).Value);
        }

        public void Dispose()
        {
            _WorkflowDbProvider.Dispose();
            _StatsDbProvider.Dispose();
        }
    }
}
