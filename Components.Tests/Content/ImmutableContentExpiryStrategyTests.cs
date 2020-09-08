using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class ImmutableContentExpiryStrategyTests
    {
        [Theory]
        [InlineData("2020-09-07T12:12:23Z", 1209600)]
        [InlineData("2020-09-07T06:38:01Z", 1)]
        [InlineData("2020-08-24T12:12:24Z", 1337)]
        [InlineData("2020-08-14T12:12:23Z", 42)]
        [InlineData("2020-08-24T12:12:23Z", 0)]
        public void Calculate_returns_the_configured_max_time_to_live_value(DateTime created, int maxTtl)
        {
            // Assemble
            var config = new TestHttpResponseHeaderConfig(maxTtl);
            var sut = new ImmutableContentExpiryStrategy(config);

            // Act
            var result = sut.Calculate(created);

            // Assert
            Assert.Equal(maxTtl, result);
        }
    }
}
