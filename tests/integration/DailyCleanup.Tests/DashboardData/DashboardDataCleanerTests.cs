// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.DashboardData;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Tests.DashboardData
{
    public abstract class DashboardDataCleanerTests
    {
        private readonly DashboardDataDbContext _dbContext;

        protected DashboardDataCleanerTests(DbContextOptions<DashboardDataDbContext> contentDbContextOptions)
        {
            _dbContext = new DashboardDataDbContext(contentDbContextOptions ?? throw new ArgumentNullException(nameof(contentDbContextOptions)));
            _dbContext.Database.EnsureCreated();
        }

        private void AddTestData(int processed, int unprocessed)
        {
            // Add requested amount of processed DashboardData rows
            for (var i = 0; i < processed; i++)
            {
                _dbContext.DashboardInputJson.Add(new DashboardInputJsonEntity
                {
                    JsonData = "This is a test",
                    DownloadedDate = DateTime.UtcNow.AddHours(-1 * i),
                    ProcessedDate = DateTime.UtcNow,
                    Hash = "This is not a hash"
                });
            }

            // Add requested amount of unprocessed DashboardData rows
            for (var i = 0; i < unprocessed; i++)
            {
                _dbContext.DashboardInputJson.Add(new DashboardInputJsonEntity
                {
                    JsonData = "This is a test",
                    DownloadedDate = DateTime.UtcNow.AddHours(-1 * i),
                    Hash = "This is not a hash"
                });
            }

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task Remove_With_All_Processed_Should_Leave_None()
        {
            var command = new RemoveProcessedDashboardDataCommand(_dbContext, new NullLogger<RemoveProcessedDashboardDataCommand>());

            AddTestData(processed: 5, unprocessed: 0);

            var result = (RemoveProcessedDashboardDataResult)await command.ExecuteAsync();

            Assert.Equal(5, result.Removed);
            Assert.Equal(0, result.Remaining);
        }
    }
}
