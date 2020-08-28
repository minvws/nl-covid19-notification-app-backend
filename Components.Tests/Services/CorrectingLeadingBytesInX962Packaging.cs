// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.Services
{
    public class CorrectingLeadingBytesInX962Packaging2
    {
        [Fact]
        public void AllZeros()
        {
            Assert.Throws<InvalidOperationException>(() => new byte[32].StripLeadingZeros());
        }

        [Fact]
        public void Shift1()
        {
            var buffer = new byte[] { 0, 1 };
            var actual = buffer.StripLeadingZeros();
            Assert.Equal(new byte[] { 1, 0 }, actual);
        }

        [Fact]
        public void ShiftMore()
        {
            var buffer = new byte[] { 0, 0, 1, 0 };
            var actual = buffer.StripLeadingZeros();
            Assert.Equal(new byte[] { 1, 0, 0, 0 }, actual);
        }

        [Fact]
        public void ShiftMore2()
        {
            var buffer = new byte[] { 0, 0, 0, 0, 0, 0, 1, 0, 1, 2, 0 };
            var actual = buffer.StripLeadingZeros();
            Assert.Equal(new byte[] { 1, 0, 1, 2, 0, 0, 0, 0, 0, 0, 0 }, actual);
        }

        [Fact]
        public void ZeroLength()
        {
            var buffer = new byte[0];
            var actual = buffer.StripLeadingZeros();
            Assert.Equal(new byte[0], actual);
        }
    }
}
