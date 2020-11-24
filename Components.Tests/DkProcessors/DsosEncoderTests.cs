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


        /// <summary>
        /// Do not bother testing derivable DatePrecision
        /// </summary>
        [InlineData(-14, SymptomStatus.Symptomatic, EncodedDsosType.NotEncoded)]
        [InlineData(21, SymptomStatus.Symptomatic, EncodedDsosType.NotEncoded)]
        
        [InlineData(22, SymptomStatus.Symptomatic, EncodedDsosType.DaysSinceLastDayOfInterval)]
        [InlineData(1949, SymptomStatus.Symptomatic, EncodedDsosType.DaysSinceLastDayOfInterval)]

        [InlineData(1986, SymptomStatus.Symptomatic, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [InlineData(2000, SymptomStatus.Symptomatic, EncodedDsosType.DaysSinceSubmissionOfKeys)]

        [InlineData(2986, SymptomStatus.Asymptomatic, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [InlineData(3000, SymptomStatus.Asymptomatic, EncodedDsosType.DaysSinceSubmissionOfKeys)]

        [InlineData(3986, SymptomStatus.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [InlineData(4000, SymptomStatus.Unknown, EncodedDsosType.DaysSinceSubmissionOfKeys)]
        [Theory]
        public void Valid(int value, SymptomStatus ss, EncodedDsosType edt)
        {
            Assert.True(_Service.TryDecode(value, out var result));
            Assert.Equal(ss, result.SymptomStatus);
            Assert.Equal(edt, result.ValueType);
        }


        [InlineData(-15, false)]
        [InlineData(-14, true)]
        [InlineData(21, true)]
        [InlineData(22, false)]
        [Theory]
        public void SymptomaticWithDsos(int value, bool encoded)
        {
            Assert.Equal(encoded, _Service.TryEncodeSymptomatic(value, out var result));
            if (!encoded) return;
            Assert.Equal(value, result);
            Assert.True(_Service.TryDecode(result, out var decodeResult));
            Assert.Equal(SymptomStatus.Symptomatic, decodeResult.SymptomStatus);
            Assert.Equal(DatePrecision.Exact, decodeResult.DatePrecision);
            Assert.Equal(1, decodeResult.IntervalDuration);
            Assert.Equal(0, decodeResult.Offset);
            Assert.Equal(EncodedDsosType.NotEncoded, decodeResult.ValueType);
        }
    }
}
