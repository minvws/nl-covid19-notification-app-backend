// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.Iks
{
    public class RemoveExpiredIksOutCommandTests : IDisposable
    {
        private readonly IksOutDbContext _iksOutDbContext;
        private static DbConnection connection;

        public RemoveExpiredIksOutCommandTests()
        {
            _iksOutDbContext = new IksOutDbContext(new DbContextOptionsBuilder<IksOutDbContext>().UseSqlite(CreateInMemoryDatabase()).Options);
            _iksOutDbContext.Database.EnsureCreated();
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
            var logger = new NullLogger<RemoveExpiredIksOutCommand>();

            // Assemble - add data up to "now"
            var firstDate = DateTime.Parse("2020-12-01T20:00:00Z");
            for (var day = 0; day < 26; day++)
            {
                _iksOutDbContext.Iks.Add(new IksOutEntity
                {
                    Created = firstDate.AddDays(day),
                    ValidFor = firstDate.AddDays(day),
                    Sent = false,
                    Content = new byte[] { 0x0 }
                });
            }
            _iksOutDbContext.SaveChanges();

            Assert.Equal(26, _iksOutDbContext.Iks.Count());

            // Act
            var command = new RemoveExpiredIksOutCommand(
                _iksOutDbContext,
                logger,
                dateTimeProvider.Object,
                configurationMock.Object
            );
            command.ExecuteAsync().GetAwaiter().GetResult();

            // Assert
            Assert.Empty(_iksOutDbContext.Iks.Where(x => x.Created < DateTime.Parse("2020-12-12T00:00:00Z")));
            Assert.Equal(15, _iksOutDbContext.Iks.Count());
        }
    }
}
