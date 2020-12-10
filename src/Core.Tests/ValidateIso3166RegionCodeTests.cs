using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Tests
{
    public class ValidateIso3166RegionCodeTests
    {
        private readonly Iso3166RegionCodeValidator _Validator = new Iso3166RegionCodeValidator();

        [InlineData("GB")] //We live in hope.
        [InlineData("BE")]
        [InlineData("GR")]
        [InlineData("LT")]
        [InlineData("PT")]
        [InlineData("BG")]
        [InlineData("ES")]
        [InlineData("LU")]
        [InlineData("RO")]
        [InlineData("CZ")]
        [InlineData("FR")]
        [InlineData("HU")]
        [InlineData("SI")]
        [InlineData("DK")]
        [InlineData("HR")]
        [InlineData("MT")]
        [InlineData("SK")]
        [InlineData("DE")]
        [InlineData("IT")]
        [InlineData("NL")]
        [InlineData("FI")]
        [InlineData("EE")]
        [InlineData("CY")]
        [InlineData("AT")]
        [InlineData("SE")]
        [InlineData("IE")]
        [InlineData("LV")]
        [InlineData("PL")]
        [InlineData("IS")]
        [InlineData("NO")]
        [InlineData("LI")]
        [InlineData("CH")]
        [Theory]
        public void Valid(string value)
        {
            Assert.True(_Validator.IsValid(value));
        }

        [InlineData("XX")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(" ,")]
        [InlineData(" FR")]
        [InlineData(" FR ")]
        [InlineData("FR,")]
        [InlineData("fr")]
        [InlineData("Fr")]
        [InlineData("FR ")]
        [InlineData("F R")]
        [InlineData(",,dsf,s,dfsFR")]
        [InlineData("1233")]
        [Theory]
        public void Invalid(string value)
        {
            Assert.False(_Validator.IsValid(value));
        }
    }
}
