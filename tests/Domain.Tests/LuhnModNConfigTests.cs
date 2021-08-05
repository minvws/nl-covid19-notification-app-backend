// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class LuhnModNConfigTests
    {
        [Fact]
        public void GoodCustomValues()
        {
            var argle = new LuhnModNConfig("BCFGJLQRSTUVXYZ23456789", 7);
            Assert.Equal(7, argle.ValueLength);
            Assert.Equal(23, argle.CharacterSet.Length);
            Assert.True(argle.CharacterSet.Contains('R'));
            Assert.True(!argle.CharacterSet.Contains('A'));
        }

        [Fact]
        public void BadLengthLo()
        {
            Assert.Throws<ArgumentException>(() => new LuhnModNConfig("abcdef", 1));
        }

        [Fact]
        public void BadCharacterSet()
        {
            Assert.Throws<ArgumentException>(() => new LuhnModNConfig("sdssdfsdf", 2));
        }

    }
}
