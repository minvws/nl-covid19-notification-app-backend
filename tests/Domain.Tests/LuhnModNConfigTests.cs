// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class LuhnModNConfigTests
    {
        [Fact]
        public void GoodCustomValues()
        {
            var config = new LuhnModNConfig("BCFGJLQRSTUVXYZ23456789", 7);
            Assert.Equal(7, config.ValueLength);
            Assert.Equal(23, config.CharacterSet.Length);
            Assert.Contains('R', config.CharacterSet);
            Assert.DoesNotContain('A', config.CharacterSet);
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
