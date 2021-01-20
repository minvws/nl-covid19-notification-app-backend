using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using System;
using System.Linq;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksInbound
{
    public class RemoveExpiredIksInCommandTests
    {
        private readonly IDbProvider<IksInDbContext> _IksInDbProvider;

        public RemoveExpiredIksInCommandTests()
        {
            _IksInDbProvider = new SqliteInMemoryDbProvider<IksInDbContext>();
        }

        [Fact]
        public void Tests_that_all_and_only_rows_older_than_14_FULL_days_are_removed()
        {
            // Assemble
            var currentDate = DateTime.Parse("2020-12-26T03:30:00Z").ToUniversalTime();
            var dateTimeProvider = new Mock<IUtcDateTimeProvider>();
            dateTimeProvider.Setup(_ => _.Snapshot).Returns(currentDate);
            var logger = new Mock<ILogger<RemoveExpiredIksInCommandLoggingExtensions>>();
            var context = _IksInDbProvider.CreateNew();

            // Assemble - add data up to "now"
            var firstDate = DateTime.Parse("2020-12-01T20:00:00Z");
            for (var day = 0; day < 26; day++)
            {
                context.InJob.Add(new IksInJobEntity
                {
                    LastBatchTag = (day+1).ToString(),
                    LastRun = firstDate.AddDays(day)
                });
            }
            context.SaveChanges();

            Assert.Equal(26, context.InJob.Count());

            // Act
            var command = new RemoveExpiredIksInCommand(
                context,
                dateTimeProvider.Object,
                new RemoveExpiredIksInCommandLoggingExtensions(logger.Object)
            );
            command.Execute();

            // Assert
            Assert.Empty(context.InJob.Where(x => x.LastRun < DateTime.Parse("2020-12-12T00:00:00Z")));
            Assert.Equal(15, context.InJob.Count());
        }
    }
}
