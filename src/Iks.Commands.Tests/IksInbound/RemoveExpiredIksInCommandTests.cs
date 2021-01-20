using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
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
            var configurationMock = new Mock<IIksCleaningConfig>();
            configurationMock.Setup(p => p.LifetimeDays).Returns(14);
            var logger = new Mock<ILogger<RemoveExpiredIksLoggingExtensions>>();
            var contextFunc = _IksInDbProvider.CreateNew;
            var context = contextFunc();
            
            // Assemble - add data up to "now"
            var firstDate = DateTime.Parse("2020-12-01T20:00:00Z");
            for (var day = 0; day < 26; day++)
            {
                context.Received.Add(new IksInEntity
                {
                    Id = day+1,
                    Created = firstDate.AddDays(day),
                });
            }
            context.SaveChanges();

            Assert.Equal(26, context.Received.Count());

            // Act
            var command = new RemoveExpiredIksInCommand(
                contextFunc,
                new RemoveExpiredIksLoggingExtensions(logger.Object),
                dateTimeProvider.Object,
                configurationMock.Object
            );
            command.ExecuteAsync();

            // Assert
            Assert.Empty(context.Received.Where(x => x.Created < DateTime.Parse("2020-12-12T00:00:00Z")));
            Assert.Equal(15, context.Received.Count());
        }
    }
}
