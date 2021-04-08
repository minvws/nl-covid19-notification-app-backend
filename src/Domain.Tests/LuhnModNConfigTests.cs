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
