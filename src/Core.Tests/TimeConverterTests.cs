// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class TimeConverterTests
    {
        [Theory]
        [InlineData(2661984, "2020-08-12T00:00:00Z")]
        [InlineData(2638224, "2020-02-29T00:00:00Z")]
        [InlineData(307296, "1975-11-05T00:00:00Z")]
        [InlineData(2757312, "2022-06-05T00:00:00Z")]

        public void ToRollingStartNumberTest(int expectedResult, string utcDateTimeString)
        {
            // Assemble
            var utcDateTime = DateTime.Parse(utcDateTimeString).ToUniversalTime();

            // Act
            var result = utcDateTime.ToRollingStartNumber();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData(2661984, "2020-08-12T00:00:00Z")]
        [InlineData(2638224, "2020-02-29T00:00:00Z")]
        [InlineData(307296, "1975-11-05T00:00:00Z")]
        [InlineData(2757312, "2022-06-05T00:00:00Z")]
        public void FromRollingStartNumberTest(int rollingStartNumber, string expectedUtcDateTimeString)
        {
            // Assemble
            var expectedUtcDateTime = DateTime.Parse(expectedUtcDateTimeString).ToUniversalTime();

            // Act
            var result = rollingStartNumber.FromRollingStartNumber();

            // Assert
            Assert.Equal(expectedUtcDateTime, result);
        }
        
        [Theory]
        [InlineData(1597213200U, "2020-08-12T06:20:00Z")]
        [InlineData(1582981993U, "2020-02-29T13:13:13Z")]
        [InlineData(184413059U, "1975-11-05T09:50:59Z")]
        [InlineData(1654443923U, "2022-06-05T15:45:23Z")]
        public void ToUnixTimeForEksEngineTest(ulong expectedResult, string inputUtcDateTimeString)
        {
            // Assemble
            var inputDate = DateTime.Parse(inputUtcDateTimeString);

            // Act
            var result = inputDate.ToUnixTimeU64();

            // Assert
            Assert.Equal(expectedResult, result);
        }
    }
}