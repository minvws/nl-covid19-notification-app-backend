// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
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
    }
}
