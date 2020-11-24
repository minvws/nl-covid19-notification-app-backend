namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public class DsosService
    {
        private const int RangeFactor = 100;
        private const int OffsetSymptomaticOnsetDateUnknown = 2000;
        private const int OffsetAsymptomatic = 3000;
        private const int OffsetSymptomStateUnknown = 4000;

        private bool ValidRange(int value) => -14 <= value && value <= 21;

        public bool TryEncodeSymptomatic(int dsos, out int encodedResult)
        {
            var result = ValidRange(dsos);
            encodedResult = dsos;
            return result;
        }

        public bool TryEncodeSymptomaticDateRange(int daysSinceLastDayOfRange, int rangeSpanDays, out int encodedResult)
        {
            //TODO Check!
            var result = 0 < daysSinceLastDayOfRange && daysSinceLastDayOfRange < 14 && 0 < rangeSpanDays && rangeSpanDays < 14; //&& beginning of range?
            encodedResult = rangeSpanDays * RangeFactor + daysSinceLastDayOfRange;
            return result;
        }

        public bool TryEncodeSymptomaticOnsetUnknown(int daysSinceSubmission, out int encodedResult)
        {
            var result = ValidRange(daysSinceSubmission);
            encodedResult = result ? daysSinceSubmission + OffsetSymptomaticOnsetDateUnknown : int.MinValue;
            return result;
        }

        public bool TryEncodeSymptomStatusUnknown(int daysSinceSubmission, out int encodedResult)
        {
            var result = ValidRange(daysSinceSubmission);
            encodedResult = result ? daysSinceSubmission + OffsetSymptomStateUnknown : int.MinValue;
            return result;
        }


        public bool TryEncodeAsymptomatic(int daysSinceSubmission, out int encodedResult)
        {
            var result = ValidRange(daysSinceSubmission);
            encodedResult = result ? daysSinceSubmission + OffsetAsymptomatic : int.MinValue;
            return result;
        }
        
        //From the JS
        public bool TryDecode(int value, out DsosDecodeResult result)
        {
            if (ValidRange(value))
            {
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Symptomatic,
                    DatePrecision = DatePrecision.Exact,
                    ValueType = EncodedDsosType.NotEncoded,
                    IntervalDuration = 1,
                    Offset = 0,
                    DecodedValue = value
                };
                return true;
            }

            if (21 < value && value < 1950)
            {
                var intervalDuration = JavascriptMaths.Round(value / 100.0); //TODO Check the intent?
                var offset = intervalDuration * 100;
                result = new DsosDecodeResult
                {
                    IntervalDuration = intervalDuration,
                    SymptomStatus = SymptomStatus.Symptomatic,
                    ValueType = EncodedDsosType.DaysSinceLastDayOfInterval,
                    DatePrecision = DatePrecision.Range,
                    Offset = offset,
                    DecodedValue = value - offset
                };
                return true;
            }

            if (value >= 1986 && value <= OffsetSymptomaticOnsetDateUnknown)
            {
                const int offset = OffsetSymptomaticOnsetDateUnknown;
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Symptomatic,
                    DatePrecision = DatePrecision.Unknown,
                    ValueType = EncodedDsosType.DaysSinceSubmissionOfKeys,
                    IntervalDuration = 1,
                    Offset = offset,
                    DecodedValue = value - offset
                };
                return true;
            }

            if (value >= 2986 && value <= OffsetAsymptomatic)
            {
                const int offset = OffsetAsymptomatic;
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Asymptomatic,
                    DatePrecision = DatePrecision.Unknown,
                    ValueType = EncodedDsosType.DaysSinceSubmissionOfKeys,
                    IntervalDuration = 1,
                    Offset = offset,
                    DecodedValue = value - offset
                };
                return true;
            }

            if (value >= 3986 && value <= OffsetSymptomStateUnknown)
            {
                const int offset = OffsetSymptomStateUnknown;
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Unknown,
                    DatePrecision = DatePrecision.Unknown,
                    ValueType = EncodedDsosType.DaysSinceSubmissionOfKeys,
                    IntervalDuration = 1,
                    Offset = offset,
                    DecodedValue = value - offset
                };
                return true;
            }

            result = new DsosDecodeResult(); //Cos of null checking in the compiler is turned on.
            return false;
        }
    }
}