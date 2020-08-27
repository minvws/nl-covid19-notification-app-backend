using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.ExposureKeySets
{
    public class ExposureKeySetCleanerTests
    {
        private class FakeDtp : IUtcDateTimeProvider
        {
            public DateTime Now() => throw new NotImplementedException(); //ncrunch: no coverage
            public DateTime TakeSnapshot() => throw new NotImplementedException(); //ncrunch: no coverage
            public DateTime Snapshot { get; set; }
        }
        private class FakeEksConfig : IEksConfig
        {
            public int LifetimeDays => 14;
            public int TekCountMax => throw new NotImplementedException(); //ncrunch: no coverage
            public int TekCountMin => throw new NotImplementedException(); //ncrunch: no coverage
            public int PageSize => throw new NotImplementedException(); //ncrunch: no coverage
            public bool CleanupDeletesData { get; set; }
        }
        private static void Add(ContentDbContext contentDbContext, int id)
        {
            contentDbContext.Content.Add(new ContentEntity
            {
                Content = new byte[0],
                PublishingId = id.ToString(),
                ContentTypeName = "meh",
                Created = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) - TimeSpan.FromDays(id),
                Release = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) - TimeSpan.FromDays(id),
                Type = ContentTypes.ExposureKeySet
            });
        }

        [Fact]
        [ExclusivelyUses("EksCleanerTests")]
        public void Cleaner()
        {
            var lf = new LoggerFactory();

            Func<ContentDbContext> dbp = () =>
            {
                var y = new DbContextOptionsBuilder();
                y.UseSqlServer("Data Source=.;Initial Catalog=EksCleanerTests;Integrated Security=True");
                return new ContentDbContext(y.Options);
            };

            var db = dbp().Database;
            db.EnsureDeleted();
            db.EnsureCreated();

            var command = new RemoveExpiredEksCommand(dbp(), new FakeEksConfig(), new StandardUtcDateTimeProvider(), lf.CreateLogger<RemoveExpiredEksCommand>());

            var result = command.Execute();

            Assert.Equal(0, result.Found);
            Assert.Equal(0, result.WalkingDead);
            Assert.Equal(0, result.Killed);
            Assert.Equal(0, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }

        [Fact]
        [ExclusivelyUses("EksCleanerTests")]
        public void NoKill()
        {
            var lf = new LoggerFactory();

            Func<ContentDbContext> dbp = () =>
            {
                var y = new DbContextOptionsBuilder();
                y.UseSqlServer("Data Source=.;Initial Catalog=EksCleanerTests;Integrated Security=True");
                return new ContentDbContext(y.Options);
            };

            var contentDbContext = dbp();
            var db = contentDbContext.Database;
            db.EnsureDeleted();
            db.EnsureCreated();

            Add(contentDbContext, 15);
            contentDbContext.SaveChanges();

            var fakeDtp = new FakeDtp() { Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) };
            var command = new RemoveExpiredEksCommand(contentDbContext, new FakeEksConfig(), fakeDtp, lf.CreateLogger<RemoveExpiredEksCommand>());

            var result = command.Execute();

            Assert.Equal(1, result.Found);
            Assert.Equal(1, result.WalkingDead);
            Assert.Equal(0, result.Killed);
            Assert.Equal(1, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }


        [Fact]
        [ExclusivelyUses("EksCleanerTests")]
        public void Kill()
        {
            var lf = new LoggerFactory();

            Func<ContentDbContext> dbp = () =>
            {
                var y = new DbContextOptionsBuilder();
                y.UseSqlServer("Data Source=.;Initial Catalog=EksCleanerTests;Integrated Security=True");
                return new ContentDbContext(y.Options);
            };

            var contentDbContext = dbp();
            var db = contentDbContext.Database;
            db.EnsureDeleted();
            db.EnsureCreated();

            Add(contentDbContext, 15);
            contentDbContext.SaveChanges();

            var fakeDtp = new FakeDtp() { Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) };
            var fakeEksConfig = new FakeEksConfig() { CleanupDeletesData = true };
            var command = new RemoveExpiredEksCommand(contentDbContext, fakeEksConfig, fakeDtp, lf.CreateLogger<RemoveExpiredEksCommand>());

            var result = command.Execute();

            Assert.Equal(1, result.Found);
            Assert.Equal(1, result.WalkingDead);
            Assert.Equal(1, result.Killed);
            Assert.Equal(0, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }


        [Fact]
        [ExclusivelyUses("EksCleanerTests")]
        public void MoreRealistic()
        {
            var lf = new LoggerFactory();

            Func<ContentDbContext> dbp = () =>
            {
                var y = new DbContextOptionsBuilder();
                y.UseSqlServer("Data Source=.;Initial Catalog=EksCleanerTests;Integrated Security=True");
                return new ContentDbContext(y.Options);
            };

            var contentDbContext = dbp();
            var db = contentDbContext.Database;
            db.EnsureDeleted();
            db.EnsureCreated();

            for (var i = 0; i < 20; i++)
                Add(contentDbContext, i);

            contentDbContext.SaveChanges();

            var fakeDtp = new FakeDtp() { Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) };
            var fakeEksConfig = new FakeEksConfig() { CleanupDeletesData = true };
            var command = new RemoveExpiredEksCommand(contentDbContext, fakeEksConfig, fakeDtp, lf.CreateLogger<RemoveExpiredEksCommand>());

            var result = command.Execute();

            Assert.Equal(20, result.Found);
            Assert.Equal(5, result.WalkingDead);
            Assert.Equal(5, result.Killed);
            Assert.Equal(15, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }

    }
}
