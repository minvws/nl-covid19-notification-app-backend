// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.Iks
{
    public class RemoveExpiredIksInCommandTests : IDisposable
    {
        private readonly IksInDbContext _iksInDbContext;
        private static DbConnection connection;

        public RemoveExpiredIksInCommandTests()
        {
            _iksInDbContext = new IksInDbContext(new DbContextOptionsBuilder<IksInDbContext>().UseSqlite(CreateInMemoryDatabase()).Options);
            _iksInDbContext.Database.EnsureCreated();
        }
        private static DbConnection CreateInMemoryDatabase()
        {
            connection = new SqliteConnection("Filename=:memory:");

            connection.Open();

            return connection;
        }

        public void Dispose() => connection.Dispose();

        [Fact]
        public void Tests_that_all_and_only_rows_older_than_14_FULL_days_are_removed()
        {
            // Assemble
            var currentDate = DateTime.Parse("2020-12-26T03:30:00Z").ToUniversalTime();
            var dateTimeProvider = new Mock<IUtcDateTimeProvider>();
            dateTimeProvider.Setup(x => x.Snapshot).Returns(currentDate);
            var configurationMock = new Mock<IIksCleaningConfig>();
            configurationMock.Setup(p => p.LifetimeDays).Returns(14);
            var logger = new Mock<ILogger<RemoveExpiredIksInCommand>>();

            // Assemble - add data up to "now"
            var firstDate = DateTime.Parse("2020-12-01T20:00:00Z");
            for (var day = 0; day < 26; day++)
            {
                _iksInDbContext.Received.Add(new IksInEntity
                {
                    Id = day + 1,
                    Created = firstDate.AddDays(day),
                });
            }
            _iksInDbContext.SaveChanges();

            Assert.Equal(26, _iksInDbContext.Received.Count());

            // Act
            var command = new RemoveExpiredIksInCommand(
                _iksInDbContext,
                logger.Object,
                dateTimeProvider.Object,
                configurationMock.Object
            );
            command.ExecuteAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Empty(_iksInDbContext.Received.Where(x => x.Created < DateTime.Parse("2020-12-12T00:00:00Z")));
            Assert.Equal(15, _iksInDbContext.Received.Count());
        }
    }
}
