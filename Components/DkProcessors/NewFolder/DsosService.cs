namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.DkProcessors
{
    public class DsosService
    {

        //From the JS
        public bool TryDecode(int value, out DsosDecodeResult result)
        {
            if (value >= -14 && value <= 21)
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

            if (value >= 1986 && value <= 2000)
            {
                const int offset = 2000;
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Symptomatic,
                    DatePrecision = DatePrecision.Unknown,
                    ValueType = EncodedDsosType.DaysSinceSubmissionOfKeys,
                    Offset = offset,
                    DecodedValue = value - offset
                };
                return true;
            }

            if (value >= 2986 && value <= 3000)
            {
                const int offset = 3000;
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Asymptomatic,
                    DatePrecision = DatePrecision.Unknown,
                    ValueType = EncodedDsosType.DaysSinceSubmissionOfKeys,
                    Offset = offset,
                    DecodedValue = value - offset
                };
                return true;
            }

            if (value >= 3986 && value <= 4000)
            {
                const int offset = 4000;
                result = new DsosDecodeResult
                {
                    SymptomStatus = SymptomStatus.Unknown,
                    DatePrecision = DatePrecision.Unknown,
                    ValueType = EncodedDsosType.DaysSinceSubmissionOfKeys,
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