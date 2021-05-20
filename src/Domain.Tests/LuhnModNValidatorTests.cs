using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.LuhnModN;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class LuhnModNValidatorTests
    {
        [InlineData("L8T6LJQ", true)]
        [InlineData("XBB5XFJ", true)]
        [InlineData("8JTRXXG", true)]
        [InlineData("H6S-NG9", false)]
        [InlineData("Z7B9EDF", false)]
        [InlineData("E4CWEGE", false)]
        [InlineData("HKV6X54", false)]
        [InlineData("86HYK@Z", false)]
        [InlineData("N3BBS!N", false)]
        [InlineData("", false)]
        [InlineData("MJTCBFA", false)]
        [InlineData(null, false)]
        [Theory]
        public void ValidateTest(string value, bool expected)
        {
            Assert.Equal(expected, new LuhnModNValidator(new LuhnModNConfig()).Validate(value));
        }

        [Fact]
        public void GenerateTest()
        {
            var c = new LuhnModNConfig();
            var r = new Random(123);
            var g = new LuhnModNGenerator(c);
            var result = g.Next(r.Next(3));
            var v = new LuhnModNValidator(c);
            Assert.True(v.Validate(result));
        }

    }
}
