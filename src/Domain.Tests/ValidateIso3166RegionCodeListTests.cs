using System;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class ValidateIso3166RegionCodeListTests
    {
        private readonly CountryCodeListParser _Parser = new CountryCodeListParser();

        [InlineData("GB", new[] { "GB" })]
        [InlineData("NL", new[] { "NL" })]
        [InlineData("nL", new[] { "NL" })] //Casing
        [InlineData("Nl", new[] { "NL" })] //Casing
        [InlineData("DE", new[] { "DE" })]
        [InlineData(" DE", new[] { "DE" })]
        [InlineData("DE ", new[] { "DE" })]
        [InlineData("DE , GB,NL,GR", new[] { "DE", "GB", "NL", "GR" })]
        [InlineData("BE,GR,LT,PT,BG,ES,LU,RO,CZ,FR,HU,SI,DK,HR,MT,SK,DE,IT,NL,FI,EE,CY,AT,SE,IE,LV,PL,IS,NO,LI,CH", 
            new[] { "BE", "GR", "LT", "PT", "BG", "ES", "LU", "RO", "CZ", "FR", "HU", "SI", "DK", "HR", "MT", "SK", "DE", "IT", "NL", "FI", "EE", "CY", "AT", "SE", "IE", "LV", "PL", "IS", "NO", "LI", "CH" })]
        [Theory]
        public void Valid(string value, string[] expected)
        {
            Assert.Equal(expected, _Parser.Parse(value));
        }

        [InlineData("G")]
        [InlineData("XX")]
        [InlineData("")] //Casing
        [InlineData("DE . GB,NL,GR")]
        [InlineData(",,,,,,          ,,DE , GB,NL,GR")]
        [Theory]
        public void Invalid(string value)
        {
            Assert.Throws<ArgumentException>(() => _Parser.Parse(value));
        }
    }
}