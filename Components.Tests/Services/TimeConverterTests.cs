using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    [TestClass]
    public class TimeConverterTests
    {
        [DataTestMethod]
        [DataRow(2661984, "2020-08-12T00:00:00Z")]
        [DataRow(2638224, "2020-02-29T00:00:00Z")]
        [DataRow(307296, "1975-11-05T00:00:00Z")]
        [DataRow(2757312, "2022-06-05T00:00:00Z")]

        public void ToRollingStartNumberTest(int expectedResult, string utcDateTimeString)
        {
            // Assemble
            var utcDateTime = DateTime.Parse(utcDateTimeString).ToUniversalTime();

            // Act
            var result = utcDateTime.ToRollingStartNumber();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [DataTestMethod]
        [DataRow(2661984, "2020-08-12T00:00:00Z")]
        [DataRow(2638224, "2020-02-29T00:00:00Z")]
        [DataRow(307296, "1975-11-05T00:00:00Z")]
        [DataRow(2757312, "2022-06-05T00:00:00Z")]
        public void FromRollingStartNumberTest(int rollingStartNumber, string expectedUtcDateTimeString)
        {
            // Assemble
            var expectedUtcDateTime = DateTime.Parse(expectedUtcDateTimeString).ToUniversalTime();

            // Act
            var result = rollingStartNumber.FromRollingStartNumber();

            // Assert
            Assert.AreEqual(expectedUtcDateTime, result);
        }
        
        [DataTestMethod]
        [DataRow(1597213200U, "2020-08-12T06:20:00Z")]
        [DataRow(1582981993U, "2020-02-29T13:13:13Z")]
        [DataRow(184413059U, "1975-11-05T09:50:59Z")]
        [DataRow(1654443923U, "2022-06-05T15:45:23Z")]
        public void ToUnixTimeForEksEngineTest(ulong expectedResult, string inputUtcDateTimeString)
        {
            // Assemble
            var inputDate = DateTime.Parse(inputUtcDateTimeString);

            // Act
            var result = inputDate.ToUnixTimeU64();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}