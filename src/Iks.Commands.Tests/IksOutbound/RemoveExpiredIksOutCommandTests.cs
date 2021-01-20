using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.TestFramework;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksOutbound
{
    public class RemoveExpiredIksOutCommandTests
    {
        private readonly IDbProvider<IksOutDbContext> _IksOutDbProvider;

        public RemoveExpiredIksOutCommandTests()
        {
            _IksOutDbProvider = new SqliteInMemoryDbProvider<IksOutDbContext>();
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
            var contextFunc = _IksOutDbProvider.CreateNew;
            var context = contextFunc();

            // Assemble - add data up to "now"
            var firstDate = DateTime.Parse("2020-12-01T20:00:00Z");
            for (var day = 0; day < 26; day++)
            {
                context.Iks.Add(new IksOutEntity
                {
                    Created = firstDate.AddDays(day),
                    ValidFor = firstDate.AddDays(day),
                    Sent = false,
                    Content = new byte[] {0x0}
                });
            }
            context.SaveChanges();

            Assert.Equal(26, context.Iks.Count());

            // Act
            var command = new RemoveExpiredIksOutCommand(
                contextFunc,
                new RemoveExpiredIksLoggingExtensions(logger.Object),
                dateTimeProvider.Object,
                configurationMock.Object
            );
            command.ExecuteAsync();

            // Assert
            Assert.Empty(context.Iks.Where(x => x.Created < DateTime.Parse("2020-12-12T00:00:00Z")));
            Assert.Equal(15, context.Iks.Count());
        }
    }
}