using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ProtocolSettings;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class EksMaxageCalculatorTests
    {
        [Theory]
        [InlineData("2020-09-07T12:12:23Z", "2020-09-07T12:12:23Z", 1227600)] //Maximum
        [InlineData("2020-09-07T00:00:00Z", "2020-09-21T00:00:00Z", 18000)] //14 days but still wait for the Cleanup
        [InlineData("2020-09-07T00:00:00Z", "2020-09-21T04:58:59Z", 61)] //Edge of min
        [InlineData("2020-09-07T00:00:00Z", "2020-09-22T04:59:00Z", 60)] //Edge of min
        [InlineData("2020-09-07T00:00:00Z", "2020-09-22T04:59:01Z", 60)] //Edge of min
        [InlineData("2020-09-07T12:12:23Z", "2021-09-07T12:12:23Z", 60)] //Minumum
        public void Calculate_returns_time_to_live_value(DateTime created, DateTime snapshot, int expected)
        {
            var dtp = new Moq.Mock<IUtcDateTimeProvider>();
            dtp.SetupGet(x => x.Snapshot).Returns(snapshot);

            var eksConfig = new Moq.Mock<IEksConfig>();
            eksConfig.SetupGet(x => x.LifetimeDays).Returns(14);

            var taskSchedConfig = new Moq.Mock<ITaskSchedulingConfig>();
            taskSchedConfig.SetupGet(x => x.DailyCleanupHoursAfterMidnight).Returns(5);

            // Assemble
            var sut = new EksMaxageCalculator(dtp.Object, eksConfig.Object, taskSchedConfig.Object);

            // Act
            var result = sut.Execute(created);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
