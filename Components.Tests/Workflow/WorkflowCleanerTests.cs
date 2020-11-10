using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredWorkflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Expiry;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Workflow.Cleanup
{
    public class WorkflowCleanerTests
    {
        private readonly RemoveExpiredWorkflowsCommand _Command;
        private readonly WorkflowDbContext _DbContext;
        private readonly FakeConfig _FakeConfig;
        private readonly FakeDtp _Dtp;

        private int _WorkflowCount;

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

        public WorkflowCleanerTests()
        {
            WorkflowDbContext Func() => new WorkflowDbContext(new DbContextOptionsBuilder().UseSqlServer("Data Source=.;Initial Catalog=WorkflowCleanerTests;Integrated Security=True").Options);

            _DbContext = Func();
            var db = _DbContext.Database;
            db.EnsureDeleted();
            db.EnsureCreated();

            _FakeConfig = new FakeConfig();
            _Dtp = new FakeDtp();

            var lf = new LoggerFactory();
            var expWorkflowLogger = new ExpiredWorkflowLoggingExtensions(lf.CreateLogger<ExpiredWorkflowLoggingExtensions>());
            _Command = new RemoveExpiredWorkflowsCommand(Func, expWorkflowLogger, _Dtp, _FakeConfig);
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void Cleaner()
        {
            var result = _Command.Execute();

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
            _Dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            Add(1, 0, null);
            _DbContext.SaveChanges();

            var result = _Command.Execute();

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
            _Dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            _FakeConfig.CleanupDeletesData = true;
            Add(1, 0, null);
            _DbContext.SaveChanges();

            var result = _Command.Execute();

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
            _Dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            _FakeConfig.CleanupDeletesData = true;
            Add(1, 0, null);
            Add(1, 0, null);
            Add(1, 14, 14);
            Add(1, 10, 5);
            Add(-10, 10, 5);
            _DbContext.SaveChanges();

            Assert.Throws<InvalidOperationException>(()=>_Command.Execute());
        }

        [Fact]
        [ExclusivelyUses("WorkflowCleanerTests")]
        public void MoreRealistic()
        {
            _Dtp.Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc);
            _FakeConfig.CleanupDeletesData = true;
            Add(1, 0, null);
            Add(1, 0, null);
            Add(1, 14, 14);
            Add(1, 10, 10);
            Add(-10, 10, 0);
            _DbContext.SaveChanges();

            var result = _Command.Execute();

            Assert.Equal(_WorkflowCount, result.Before.Count);
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
            ++_WorkflowCount;

            var v = _Dtp.Snapshot - TimeSpan.FromHours(offsetHours);

            var w = new TekReleaseWorkflowStateEntity
            {
                Created = _Dtp.Snapshot, //Doesnt matter
                BucketId = new[] { (byte)_WorkflowCount },
                ConfirmationKey = new[] { (byte)_WorkflowCount },
                LabConfirmationId = publishedCount.HasValue ? null : _WorkflowCount.ToString(),
                PollToken = publishedCount.HasValue ? _WorkflowCount.ToString() : null,
                AuthorisedByCaregiver = publishedCount.HasValue ? _Dtp.Snapshot : (DateTime?)null,
                DateOfSymptomsOnset = publishedCount.HasValue ? _Dtp.Snapshot : (DateTime?)null,
                ValidUntil = v
            };

            _DbContext.KeyReleaseWorkflowStates.Add(w);

            for (var i = 0; i < tekCount; i++)
            {
                var t = new TekEntity
                {
                    Owner = w,
                    RollingStartNumber = 0,
                    RollingPeriod = 0,
                    PublishAfter = _Dtp.Snapshot,
                    KeyData = new byte[0],
                    PublishingState = i < (publishedCount ?? 0) ? PublishingState.Published : PublishingState.Unpublished,
                    Region = "NL"
                };
                _DbContext.TemporaryExposureKeys.Add(t);
            }
        }
    }
}
