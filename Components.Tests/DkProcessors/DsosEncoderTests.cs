using System;
using System.Collections.Generic;
using System.Text;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Tests.DkProcessors
{
    public class DsosDecoderTests
    {
        //JS Math.Round:
        // console.log(Math.round(5.95), Math.round(5.5), Math.round(5.05));
        // expected output: 6 6 5
        // console.log(Math.round(-5.05), Math.round(-5.5), Math.round(-5.95));
        // expected output: -5 -5 -6
        // Was Math.Round(input_dsos / 100)
        [InlineData(5.95, 6)]
        [InlineData(5.5, 6)]
        [InlineData(5.05, 5)]
        [InlineData(-5.05, -5)]
        [InlineData(-5.5, -5)]
        [InlineData(-5.95, -6)]
        [Theory]
        public void EquivalentJsRounding(double input, int expected)
        {
            Assert.Equal(expected, JavascriptMaths.Round(input));
        }

        private readonly DsosService _Service = new DsosService();

        [InlineData(int.MinValue)]
        [InlineData(-15)]
        [InlineData(1950)]
        [InlineData(1985)]
        [InlineData(2001)]
        [InlineData(2985)]
        [InlineData(3001)]
        [InlineData(3985)]
        [InlineData(4001)]
        [InlineData(int.MaxValue)]
        [Theory]
        public void Invalid(int value)
        {
            Assert.False(_Service.TryDecode(value, out _));
        }

        [InlineData(-14)]
        [InlineData(21)]
        [InlineData(22)]
        [InlineData(1949)]
        [InlineData(1986)]
        [InlineData(2000)]
        [InlineData(2986)]
        [InlineData(3000)]
        [InlineData(3986)]
        [InlineData(4000)]
        [Theory]
        public void SimpleValid(int value)
        {
            Assert.True(_Service.TryDecode(value, out var result));
            //Invariants - one of DatePrecision and EncodedDsosType is redundant
            Assert.False(result.DatePrecision == DatePrecision.Exact ^ result.ValueType == EncodedDsosType.NotEncoded);
            Assert.False(result.DatePrecision == DatePrecision.Range ^ result.ValueType == EncodedDsosType.DaysSinceLastDayOfInterval);
            Assert.False(result.DatePrecision == DatePrecision.Unknown ^ result.ValueType == EncodedDsosType.DaysSinceSubmissionOfKeys);
        }

        [InlineData(-14, SymptomStatus.Symptomatic, DatePrecision.Exact, EncodedDsosType.NotEncoded)]
        [InlineData(21, SymptomStatus.Symptomatic, DatePrecision.Exact, EncodedDsosType.NotEncoded)]
        
        [InlineData(22, SymptomStatus.Symptomatic, DatePrecision.Range, EncodedDsosType.DaysSinceLastDayOfInterval)]
        [InlineData(1949, SymptomStatus.Symptomatic, DatePrecision.Range, EncodedDsosType.DaysSinceLastDayOfInterval)]

        [InlineData(1986, SymptomStatus.Symptomatic, DatePrecision.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [InlineData(2000, SymptomStatus.Symptomatic, DatePrecision.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]

        [InlineData(2986, SymptomStatus.Asymptomatic, DatePrecision.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [InlineData(3000, SymptomStatus.Asymptomatic, DatePrecision.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        
        [InlineData(3986, SymptomStatus.Unknown, DatePrecision.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [InlineData(4000, SymptomStatus.Unknown, DatePrecision.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [Theory]
        public void Valid(int value, SymptomStatus ss, DatePrecision dp, EncodedDsosType edt)
        {
            Assert.True(_Service.TryDecode(value, out var result));
            Assert.Equal(ss, result.SymptomStatus);
            Assert.Equal(dp, result.DatePrecision);
            Assert.Equal(edt, result.ValueType);
        }
    }
}
