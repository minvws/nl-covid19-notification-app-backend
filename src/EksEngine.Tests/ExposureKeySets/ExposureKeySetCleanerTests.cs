// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NCrunch.Framework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Tests.ExposureKeySets
{
    public abstract class ExposureKeySetCleanerTests : IDisposable
    {
        private readonly IDbProvider<ContentDbContext> _ContentDbProvider;

        protected ExposureKeySetCleanerTests(IDbProvider<ContentDbContext> contentDbProvider)
        {
            _ContentDbProvider = contentDbProvider ?? throw new ArgumentNullException(nameof(contentDbProvider));
        }

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
        [ExclusivelyUses(nameof(ExposureKeySetCleanerTests))]
        public void Cleaner()
        {
            var lf = new LoggerFactory();
            var expEksLogger = new ExpiredEksLoggingExtensions(lf.CreateLogger<ExpiredEksLoggingExtensions>());

            var command = new RemoveExpiredEksCommand(_ContentDbProvider.CreateNew(), new FakeEksConfig(), new StandardUtcDateTimeProvider(), expEksLogger);

            var result = command.Execute();

            Assert.Equal(0, result.Found);
            Assert.Equal(0, result.Zombies);
            Assert.Equal(0, result.GivenMercy);
            Assert.Equal(0, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }

        [Fact]
        [ExclusivelyUses(nameof(ExposureKeySetCleanerTests))]
        public void NoKill()
        {
            var lf = new LoggerFactory();
            var expEksLogger = new ExpiredEksLoggingExtensions(lf.CreateLogger<ExpiredEksLoggingExtensions>());

            var contentDbContext = _ContentDbProvider.CreateNew();

            Add(contentDbContext, 15);
            contentDbContext.SaveChanges();

            var fakeDtp = new FakeDtp() { Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) };
            var command = new RemoveExpiredEksCommand(contentDbContext, new FakeEksConfig(), fakeDtp, expEksLogger);

            var result = command.Execute();

            Assert.Equal(1, result.Found);
            Assert.Equal(1, result.Zombies);
            Assert.Equal(0, result.GivenMercy);
            Assert.Equal(1, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }


        [Fact]
        [ExclusivelyUses(nameof(ExposureKeySetCleanerTests))]
        public void Kill()
        {
            var lf = new LoggerFactory();
            var expEksLogger = new ExpiredEksLoggingExtensions(lf.CreateLogger<ExpiredEksLoggingExtensions>());

            var contentDbContext = _ContentDbProvider.CreateNew();

            Add(contentDbContext, 15);
            contentDbContext.SaveChanges();

            var fakeDtp = new FakeDtp() { Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) };
            var fakeEksConfig = new FakeEksConfig() { CleanupDeletesData = true };
            var command = new RemoveExpiredEksCommand(contentDbContext, fakeEksConfig, fakeDtp, expEksLogger);

            var result = command.Execute();

            Assert.Equal(1, result.Found);
            Assert.Equal(1, result.Zombies);
            Assert.Equal(1, result.GivenMercy);
            Assert.Equal(0, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }


        [Fact]
        [ExclusivelyUses(nameof(ExposureKeySetCleanerTests))]
        public void MoreRealistic()
        {
            var lf = new LoggerFactory();
            var expEksLogger = new ExpiredEksLoggingExtensions(lf.CreateLogger<ExpiredEksLoggingExtensions>());

            var contentDbContext = _ContentDbProvider.CreateNew();

            for (var i = 0; i < 20; i++)
                Add(contentDbContext, i);

            contentDbContext.SaveChanges();

            var fakeDtp = new FakeDtp() { Snapshot = new DateTime(2020, 6, 20, 0, 0, 0, DateTimeKind.Utc) };
            var fakeEksConfig = new FakeEksConfig() { CleanupDeletesData = true };
            var command = new RemoveExpiredEksCommand(contentDbContext, fakeEksConfig, fakeDtp, expEksLogger);

            var result = command.Execute();

            Assert.Equal(20, result.Found);
            Assert.Equal(5, result.Zombies);
            Assert.Equal(5, result.GivenMercy);
            Assert.Equal(15, result.Remaining);
            Assert.Equal(0, result.Reconciliation);
        }

        public void Dispose()
        {
            _ContentDbProvider.Dispose();
        }
    }
}
