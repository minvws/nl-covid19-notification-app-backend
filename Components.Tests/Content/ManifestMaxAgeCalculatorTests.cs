using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class ManifestMaxAgeCalculatorTests
    {
        [Theory]
        [InlineData("2020-08-24T12:00:00Z", "2020-08-24T12:00:00Z", 7200)]
        [InlineData("2020-08-24T12:00:00Z", "2020-08-24T12:20:00Z", 6000)]
        [InlineData("2020-08-24T12:00:00Z", "2020-09-24T12:00:00Z", 60)]
        public void Calculate_returns_dynamically_calculated_TTL_as_a_positive_integer(DateTime created, DateTime snapshot, int expectedTtl)
        {
            // Assemble
            var dtp = new Moq.Mock<IUtcDateTimeProvider>();
            dtp.SetupGet(x => x.Snapshot).Returns(snapshot);

            var taskSchedConfig = new Moq.Mock<ITaskSchedulingConfig>();
            taskSchedConfig.SetupGet(x => x.ManifestPeriodMinutes).Returns(120); //== 7200s

            var sut = new ManifestMaxAgeCalculator(dtp.Object, taskSchedConfig.Object);

            // Act
            var result = sut.Execute(created);

            // Assert
            Assert.Equal(expectedTtl, result);
        }
    }
}
