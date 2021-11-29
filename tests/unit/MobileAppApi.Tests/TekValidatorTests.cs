// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections;
using System.Collections.Generic;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Tests
{
    public class TekValidatorTests
    {
        [Theory]
        [InlineData(0, 0, 0, 0, true)]
        [InlineData(1, 0, 0, 0, false)]
        [InlineData(0, 0, 1, 0, false)]
        [InlineData(0, 1, 0, 1, true)]
        [InlineData(1, 0, 1, 0, true)]
        [InlineData(1000, 144, 1000, 144, true)]
        [InlineData(1000, 144, 1000, 143, false)]
        public void StartHere(int leftStart, int leftPeriod, int rightStart, int rightPeriod, bool overlaps)
        {
            Assert.Equal(overlaps, new Tek { RollingStartNumber = leftStart, RollingPeriod = leftPeriod }
                .SameTime(new Tek { RollingStartNumber = rightStart, RollingPeriod = rightPeriod }));
        }

        [Theory]
        [InlineData(0, 1)] // RollingStartNumber is today and RollingPeriod is min value
        [InlineData(0, 144)] // RollingStartNumber is today and RollingPeriod is max value
        [InlineData(-14, 1)] // RollingStartNumber is max days in the past and RollingPeriod is min value
        public void Teks_Edgecases_Are_Valid(int addDays, int rollingPeriod)
        {
            // Arrange
            var rollingStartNumber = DateTime.UtcNow.Date.AddDays(addDays).ToRollingStartNumber();
            var config = new DefaultTekValidatorConfig();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var filter = new TekValidPeriodFilter(config, dateTimeProvider);

            var teks = new List<Tek>
            {
                new Tek { RollingStartNumber = rollingStartNumber, RollingPeriod = rollingPeriod }
            };

            // Act
            var result = filter.Execute(teks.ToArray());

            // Assert
            Assert.Single(result.Items);
            Assert.Empty(result.Messages);
        }

        [Theory]
        [InlineData(0, 0)] // RollingStartNumber is today and RollingPeriod is below min value
        [InlineData(0, 145)] // RollingStartNumber is today and RollingPeriod is above max value
        [InlineData(-15, 1)] // RollingStartNumber is before max days in the past and RollingPeriod is min value
        [InlineData(1, 1)] // RollingStartNumber is in the future and RollingPeriod is min value
        public void Teks_Edgecases_Are_Invalid(int addDays, int rollingPeriod)
        {
            // Arrange
            var rollingStartNumber = DateTime.UtcNow.Date.AddDays(addDays).ToRollingStartNumber();
            var config = new DefaultTekValidatorConfig();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var filter = new TekValidPeriodFilter(config, dateTimeProvider);

            var teks = new List<Tek>
            {
                new Tek { RollingStartNumber = rollingStartNumber, RollingPeriod = rollingPeriod }
            };

            // Act
            var result = filter.Execute(teks.ToArray());

            // Assert
            Assert.Empty(result.Items);
            Assert.Single(result.Messages);
        }

        [Theory]
        [ClassData(typeof(RollingStartNumberTestData))] // Allowed RollingStartNumber range in days from today
        public void Teks_Have_RollingStartNumber_Within_Range(int dayOffset)
        {
            // Arrange
            var rollingStartNumber = DateTime.UtcNow.Date.AddDays(dayOffset).ToRollingStartNumber();
            var config = new DefaultTekValidatorConfig();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var filter = new TekValidPeriodFilter(config, dateTimeProvider);

            var teks = new List<Tek>
            {
                new Tek { RollingStartNumber = rollingStartNumber, RollingPeriod = 1 }
            };

            // Act
            var result = filter.Execute(teks.ToArray());

            // Assert
            Assert.Single(result.Items);
            Assert.Empty(result.Messages);
        }

        [Fact]
        public void Teks_Have_RollingStartNumber_And_RollingPeriod_Within_Range()
        {
            // Arrange
            var config = new DefaultTekValidatorConfig();
            var dateTimeProvider = new StandardUtcDateTimeProvider();
            var filter = new TekValidPeriodFilter(config, dateTimeProvider);
            var teks = new List<Tek>();

            for (var dayOffset = -(config.MaxAgeDays); dayOffset >= 0; dayOffset++)
            {
                var rollingStartNumber = DateTime.UtcNow.Date.AddDays(0).ToRollingStartNumber();

                for (var rollingPeriod = 1; rollingPeriod <= 144; rollingPeriod++)
                {
                    teks.Add(new Tek { RollingStartNumber = rollingStartNumber, RollingPeriod = rollingPeriod });
                }
            }

            // Act
            var result = filter.Execute(teks.ToArray());

            // Assert
            Assert.Equal(teks.Count, result.Items.Length);
            Assert.Empty(result.Messages);
        }

        private class RollingStartNumberTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { -14 };
                yield return new object[] { -13 };
                yield return new object[] { -12 };
                yield return new object[] { -11 };
                yield return new object[] { -10 };
                yield return new object[] { -9 };
                yield return new object[] { -8 };
                yield return new object[] { -7 };
                yield return new object[] { -6 };
                yield return new object[] { -5 };
                yield return new object[] { -4 };
                yield return new object[] { -3 };
                yield return new object[] { -2 };
                yield return new object[] { -1 };
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
