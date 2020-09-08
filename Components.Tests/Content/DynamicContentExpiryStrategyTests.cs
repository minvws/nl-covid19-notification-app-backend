using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class DynamicContentExpiryStrategyTests
    {
        [Theory]
        [InlineData("2020-09-07T12:12:23Z", "2020-09-07T12:12:23Z", 1209600)]
        [InlineData("2020-09-07T06:38:01Z", "2020-09-07T12:12:23Z", 1189538)]
        [InlineData("2020-08-24T12:12:24Z", "2020-09-07T12:12:23Z", 1)]
        [InlineData("2020-08-14T12:12:23Z", "2020-09-07T12:12:23Z", 0)]
        [InlineData("2020-08-24T12:12:23Z", "2020-09-07T12:12:23Z", 0)]
        public void Calculate_returns_dynamically_calculated_TTL_as_a_positive_integer(DateTime created, DateTime now, int expectedTtl)
        {
            // Assemble
            var dateTimeProvider = new TestUtcDateTimeProvider(now); 
            var sut = new DynamicContentExpiryStrategy(dateTimeProvider, new TestHttpResponseHeaderConfig());

            // Act
            var result = sut.Calculate(created);

            // Assert
            Assert.Equal(expectedTtl, result);
        }
    }
}
