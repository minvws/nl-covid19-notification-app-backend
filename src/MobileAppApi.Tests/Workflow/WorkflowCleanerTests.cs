// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests.Workflow
{
    public abstract class WorkflowCleanerTests
    {
        private readonly WorkflowDbContext _workflowDbContext;

        protected WorkflowCleanerTests(DbContextOptions<WorkflowDbContext> workflowDbContextOptions)
        {
            _workflowDbContext = new WorkflowDbContext(workflowDbContextOptions);
            _workflowDbContext.Database.EnsureCreated();

            _fakeConfig = new FakeConfig();
            _dtp = new FakeDtp();

            var lf = new LoggerFactory();
            var expWorkflowLogger = new ExpiredWorkflowLoggingExtensions(lf.CreateLogger<ExpiredWorkflowLoggingExtensions>());
            _command = new RemoveExpiredWorkflowsCommand(_workflowDbContext, expWorkflowLogger, _dtp, _fakeConfig);
        }

        private readonly RemoveExpiredWorkflowsCommand _command;
        private readonly FakeConfig _fakeConfig;
        private readonly FakeDtp _dtp;

        private int _workflowCount;

        private class FakeConfig : IWorkflowConfig
        {
            public int TimeToLiveMinutes => throw new NotImplementedException(); //ncrunch: no coverage
            public int PermittedMobileDeviceClockErrorMinutes => throw new NotImplementedException(); //ncrunch: no coverage
            public int PostKeysSignatureLength => throw new NotImplementedException(); //ncrunch: no coverage
            public int BucketIdLength => throw new NotImplementedException(); //ncrunch: no coverage
            public int ConfirmationKeyLength => throw new NotImplementedException(); //ncrunch: no coverage
            public bool CleanupDeletesData { get; set; }
        }
        private class FakeDtp : IUtcDateTimeProvider
        {
            public DateTime Now() => throw new NotImplementedException(); //ncrunch: no coverage
            public DateTime TakeSnapshot() => throw new NotImplementedException(); //ncrunch: no coverage
            public DateTime Snapshot { get; set; }
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void Cleaner()
        {
            var result = _command.Execute();

            Assert.Equal(0, result.Before.Count);
            Assert.Equal(0, result.Before.Expired);
            Assert.Equal(0, result.Before.Unauthorised);
            Assert.Equal(0, result.Before.Authorised);
            Assert.Equal(0, result.Before.AuthorisedAndFullyPublished);
            Assert.Equal(0, result.Before.TekCount);
            Assert.Equal(0, result.Before.TekPublished);
            Assert.Equal(0, result.Before.TekUnpublished);

            Assert.Equal(0, result.GivenMercy);

            Assert.Equal(0, result.After.Count);
            Assert.Equal(0, result.After.Expired);
            Assert.Equal(0, result.After.Unauthorised);
            Assert.Equal(0, result.After.Authorised);
            Assert.Equal(0, result.After.AuthorisedAndFullyPublished);
            Assert.Equal(0, result.After.TekCount);
            Assert.Equal(0, result.After.TekPublished);
            Assert.Equal(0, result.After.TekUnpublished);
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void NoKill()
        {
            _dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            Add(1, 0, null);
            _workflowDbContext.SaveChanges();

            var result = _command.Execute();

            Assert.Equal(1, result.Before.Count);
            Assert.Equal(1, result.Before.Expired);
            Assert.Equal(1, result.Before.Unauthorised);
            Assert.Equal(0, result.Before.Authorised);
            Assert.Equal(0, result.Before.AuthorisedAndFullyPublished);
            Assert.Equal(0, result.Before.TekCount);
            Assert.Equal(0, result.Before.TekPublished);
            Assert.Equal(0, result.Before.TekUnpublished);

            Assert.Equal(0, result.GivenMercy);

            Assert.Equal(0, result.After.Count);
            Assert.Equal(0, result.After.Expired);
            Assert.Equal(0, result.After.Unauthorised);
            Assert.Equal(0, result.After.Authorised);
            Assert.Equal(0, result.After.AuthorisedAndFullyPublished);
            Assert.Equal(0, result.After.TekCount);
            Assert.Equal(0, result.After.TekPublished);
            Assert.Equal(0, result.After.TekUnpublished);
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void Kill()
        {
            _dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            _fakeConfig.CleanupDeletesData = true;
            Add(1, 0, null);
            _workflowDbContext.SaveChanges();

            var result = _command.Execute();

            Assert.Equal(1, result.Before.Count);
            Assert.Equal(1, result.Before.Expired);
            Assert.Equal(1, result.Before.Unauthorised);
            Assert.Equal(0, result.Before.Authorised);
            Assert.Equal(0, result.Before.AuthorisedAndFullyPublished);
            Assert.Equal(0, result.Before.TekCount);
            Assert.Equal(0, result.Before.TekPublished);
            Assert.Equal(0, result.Before.TekUnpublished);

            Assert.Equal(1, result.GivenMercy);

            Assert.Equal(0, result.After.Count);
            Assert.Equal(0, result.After.Expired);
            Assert.Equal(0, result.After.Unauthorised);
            Assert.Equal(0, result.After.Authorised);
            Assert.Equal(0, result.After.AuthorisedAndFullyPublished);
            Assert.Equal(0, result.After.TekCount);
            Assert.Equal(0, result.After.TekPublished);
            Assert.Equal(0, result.After.TekUnpublished);
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void Abort()
        {
            _dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            _fakeConfig.CleanupDeletesData = true;
            Add(1, 0, null);
            Add(1, 0, null);
            Add(1, 14, 14);
            Add(1, 10, 5);
            Add(-10, 10, 5);
            _workflowDbContext.SaveChanges();

            Assert.Throws<InvalidOperationException>(() => _command.Execute());
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void MoreRealistic()
        {
            _dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            _fakeConfig.CleanupDeletesData = true;
            Add(1, 0, null);
            Add(1, 0, null);
            Add(1, 14, 14);
            Add(1, 10, 10);
            Add(-10, 10, 0);
            _workflowDbContext.SaveChanges();

            var result = _command.Execute();

            Assert.Equal(_workflowCount, result.Before.Count);
            Assert.Equal(4, result.Before.Expired);
            Assert.Equal(2, result.Before.Unauthorised);
            Assert.Equal(2, result.Before.Authorised);
            Assert.Equal(2, result.Before.AuthorisedAndFullyPublished);
            Assert.Equal(34, result.Before.TekCount);
            Assert.Equal(24, result.Before.TekPublished);
            Assert.Equal(10, result.Before.TekUnpublished);

            Assert.Equal(4, result.GivenMercy);

            Assert.Equal(1, result.After.Count);
            Assert.Equal(0, result.After.Expired);
            Assert.Equal(0, result.After.Unauthorised);
            Assert.Equal(0, result.After.Authorised);
            Assert.Equal(0, result.After.AuthorisedAndFullyPublished);
            Assert.Equal(10, result.After.TekCount);
            Assert.Equal(0, result.After.TekPublished);
            Assert.Equal(10, result.After.TekUnpublished);
        }

        private void Add(int offsetHours, int tekCount, int? publishedCount)
        {
            ++_workflowCount;

            var v = _dtp.Snapshot - TimeSpan.FromHours(offsetHours);

            var w = new TekReleaseWorkflowStateEntity
            {
                Created = _dtp.Snapshot, //Doesnt matter
                BucketId = new[] { (byte)_workflowCount },
                ConfirmationKey = new[] { (byte)_workflowCount },
                LabConfirmationId = publishedCount.HasValue ? null : _workflowCount.ToString(),
                PollToken = publishedCount.HasValue ? _workflowCount.ToString() : null,
                AuthorisedByCaregiver = publishedCount.HasValue ? _dtp.Snapshot : (DateTime?)null,
                StartDateOfTekInclusion = publishedCount.HasValue ? _dtp.Snapshot : (DateTime?)null,
                ValidUntil = v
            };

            _workflowDbContext.KeyReleaseWorkflowStates.Add(w);

            for (var i = 0; i < tekCount; i++)
            {
                var t = new TekEntity
                {
                    Owner = w,
                    RollingStartNumber = 0,
                    RollingPeriod = 0,
                    PublishAfter = _dtp.Snapshot,
                    KeyData = new byte[0],
                    PublishingState = i < (publishedCount ?? 0) ? PublishingState.Published : PublishingState.Unpublished,
                };
                _workflowDbContext.TemporaryExposureKeys.Add(t);
            }
        }
    }
}
