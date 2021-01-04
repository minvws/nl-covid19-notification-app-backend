// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class RandomNumberTests
    {
        [Fact]
        public void Valid()
        {
            var random = new StandardRandomNumberGenerator();
            const int minValue = 1;
            const int maxValue = 99999999;

            var result = random.Next(minValue, maxValue);

            Assert.True(result >= minValue);
            Assert.True(result <= maxValue);
        }

        [Fact]
        public void GetZero()
        {
            var zeros = 0;
            var ones = 0;
            var r = new StandardRandomNumberGenerator();
            for (var i = 0; i < 1000; i++)
            {
                var next = r.Next(0, 1);
                Assert.True(next == 0 || next == 1);
                if (next == 0) zeros++;
                if (next == 1) ones++;
            }

            Assert.True(zeros > 0);
            Assert.True(ones > 0);
        }

        [Fact]
        public void RangeTest()
        {
            var random = new StandardRandomNumberGenerator();
            const int minValue = 8;
            const int maxValue = 5;

            Assert.Throws<ArgumentOutOfRangeException>(() => random.Next(minValue, maxValue));
        } //ncrunch: no coverage
    }
}
