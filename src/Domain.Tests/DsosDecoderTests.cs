using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding;
using Xunit;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Tests
{
    public class DsosDecoderTests
    {
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

        private readonly DsosEncodingService _EncodingService = new DsosEncodingService();

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
            Assert.False(_EncodingService.TryDecode(value, out _));
        }

        [InlineData(-14)]
        [InlineData(21)]
        [InlineData(2000)]
        [InlineData(2986)]
        [InlineData(3000)]
        [InlineData(3986)]
        [InlineData(4000)]
        [Theory]
        public void SimpleValid(int value)
        {
            Assert.True(_EncodingService.TryDecode(value, out var _));
        }

        [InlineData(-14)]
        [InlineData(0)]
        [InlineData(21)]
        [Theory]
        public void SymptomaticExact(int value)
        {
            Assert.True(_EncodingService.TryDecode(value, out var result));
            Assert.Equal(SymptomObservation.Symptomatic, result.SymptomObservation);
            Assert.Throws<InvalidOperationException>(() => result.DaysSinceSubmission);

            var symptomatic = result.AsSymptomatic();
            Assert.Equal(value, symptomatic.DaysSinceOnsetOfSymptoms);
            Assert.Throws<InvalidOperationException>(() => symptomatic.DaysSinceLastSymptoms);
        }

        [InlineData(-14)]
        [InlineData(0)]
        [InlineData(21)]
        [Theory]
        public void SymptomaticExactWithEncode(int original)
        {
            Assert.True(_EncodingService.TryEncodeSymptomatic(original, out var value));

            Assert.True(_EncodingService.TryDecode(value, out var result));
            Assert.Equal(SymptomObservation.Symptomatic, result.SymptomObservation);
            Assert.Throws<InvalidOperationException>(() => result.DaysSinceSubmission);

            var symptomatic = result.AsSymptomatic();
            Assert.Equal(value, symptomatic.DaysSinceOnsetOfSymptoms);
            Assert.Throws<InvalidOperationException>(() => symptomatic.DaysSinceLastSymptoms);
        }

        [InlineData(86, -14, -14)] //Matches JS implementation
        [InlineData(87, -13, -13)] //Matches JS implementation
        [InlineData(100, 0, 0)] //Matches JS implementation
        [InlineData(121, 21, 21)] //Matches JS implementation

        [InlineData(186, -15, -14)] //Matches JS implementation
        [InlineData(187, -14, -13)] //Matches JS implementation
        [InlineData(200, -1, 0)] //Matches JS implementation
        [InlineData(221, 20, 21)] //Matches JS implementation

        [Theory]
        public void Parse(int value, int lastDay, int duration)
        {
            var result = _EncodingService.ParseToRange(value);
            Assert.Equal(new Range<int>(lastDay, duration), result);
        }

        [InlineData(86, -14, -14)] //Min
        [InlineData(87, -13, -13)]
        [InlineData(100, 0, 0)]
        [InlineData(121, 21, 21)]
        [InlineData(186, -15, -14)]
        [InlineData(187, -14, -13)]
        [InlineData(200, -1, 0)]
        [InlineData(221, 20, 21)]
        [InlineData(986, -23, -14)]
        [InlineData(1586, -29, -14)]
        [InlineData(1921, 3, 21)] //Max
        [Theory]
        public void SymptomaticRange(int value, int lo, int hi)
        {
            Assert.True(_EncodingService.TryDecode(value, out var result));
            Assert.Equal(SymptomObservation.Symptomatic, result.SymptomObservation);
            Assert.Throws<InvalidOperationException>(() => result.DaysSinceSubmission);

            var symptomatic = result.AsSymptomatic();
            Assert.Throws<InvalidOperationException>(() => symptomatic.DaysSinceOnsetOfSymptoms);
            Assert.Equal(lo, symptomatic.DaysSinceLastSymptoms.Lo); //First known day of symptoms relative to TEK RSN
            Assert.Equal(hi, symptomatic.DaysSinceLastSymptoms.Hi); //First known day of symptoms relative to TEK RSN
        }


        [InlineData(2000, 0)]
        [Theory]
        public void SymptomaticOnsetUnknown(int value, int expected)
        {
            Assert.True(_EncodingService.TryDecode(value, out var result));
            Assert.Equal(SymptomObservation.Symptomatic, result.SymptomObservation);
            Assert.Equal(expected, result.DaysSinceSubmission);

            var symptomatic = result.AsSymptomatic();
            Assert.Equal(SymptomsOnsetDatePrecision.Unknown, symptomatic.SymptomsOnsetDatePrecision);
            Assert.Equal(expected, symptomatic.DaysSinceSubmission);
            Assert.Throws<InvalidOperationException>(() => symptomatic.DaysSinceOnsetOfSymptoms);
            Assert.Throws<InvalidOperationException>(() => symptomatic.DaysSinceLastSymptoms);
        }

        [InlineData(3000, 0)]
        [Theory]
        public void Asymptomatic(int value, int expected)
        {
            Assert.True(_EncodingService.TryDecode(value, out var result));
            Assert.Equal(SymptomObservation.Asymptomatic, result.SymptomObservation);
            Assert.Equal(expected, result.DaysSinceSubmission);
            Assert.Throws<InvalidOperationException>(() => result.AsSymptomatic());
        }

        [InlineData(4000, 0)]
        [Theory]
        public void SymptomaticnessAllABitVague(int value, int expected)
        {
            Assert.True(_EncodingService.TryDecode(value, out var result));
            Assert.Equal(SymptomObservation.Unknown, result.SymptomObservation);
            Assert.Equal(expected, result.DaysSinceSubmission);
            Assert.Throws<InvalidOperationException>(() => result.AsSymptomatic());
        }
    }
}
