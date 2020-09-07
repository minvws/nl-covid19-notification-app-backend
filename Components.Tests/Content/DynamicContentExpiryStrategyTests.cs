using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Content
{
    public class DynamicContentExpiryStrategyTests
    {
        [Theory]
        [InlineData("2020-09-07T12:12:23Z", "2020-09-07T12:12:23Z", "public, immutable, max-age=1209600, s-maxage=1209600")]
        [InlineData("2020-09-07T06:38:01Z", "2020-09-07T12:12:23Z", "public, immutable, max-age=1189538, s-maxage=1189538")]
        [InlineData("2020-08-24T12:12:24Z", "2020-09-07T12:12:23Z", "public, immutable, max-age=1, s-maxage=1")]
        public void Apply_returns_Active_and_expected_headers_for_active_content(DateTime created, DateTime now, string expectedHeader)
        {
            // Assemble
            var dateTimeProvider = new TestUtcDateTimeProvider(now); 
            var sut = new DynamicContentExpiryStrategy(dateTimeProvider);
            var hd = new HeaderDictionary();

            // Act
            var result = sut.Apply(created, hd);

            // Assert
            Assert.Equal(ExpiryStatus.Active, result);
            Assert.Equal(expectedHeader, hd["cache-control"]);
        }

        [Theory]
        [InlineData("2020-08-14T12:12:23Z", "2020-09-07T12:12:23Z")]
        [InlineData("2020-08-24T12:12:23Z", "2020-09-07T12:12:23Z")]
        public void Apply_returns_Expired_for_expired_content(DateTime created, DateTime now)
        {
            // Assemble
            var dateTimeProvider = new TestUtcDateTimeProvider(now); 
            var sut = new DynamicContentExpiryStrategy(dateTimeProvider);
            var hd = new HeaderDictionary();

            // Act
            var result = sut.Apply(created, hd);

            // Assert
            Assert.Equal(ExpiryStatus.Expired, result);
        }
        
        public class TestUtcDateTimeProvider : IUtcDateTimeProvider
        {
            private readonly DateTime _Time;

            public TestUtcDateTimeProvider(DateTime time)
            {
                _Time = time;
            }
            
            public DateTime Now()
            {
                return _Time;
            }

            public DateTime Snapshot { get; }
        }
    }
}
