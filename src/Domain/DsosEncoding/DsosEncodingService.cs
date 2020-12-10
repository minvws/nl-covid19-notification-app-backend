using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding
{
    public class DsosEncodingService
    {
        public static readonly Range<int> ValidRange = new Range<int>(-14, 21);

        private const int RangeFactor = 100;
        private const int OffsetSymptomaticOnsetDateUnknown = 2000;
        private const int OffsetAsymptomatic = 3000;
        private const int OffsetSymptomStateUnknown = 4000;

        public int EncodeRange(int daysSinceLastDayOfInterval, int durationDays)
        {
            return durationDays * 100 + daysSinceLastDayOfInterval;
        }

        public int EncodeSymptomaticOnsetDateUnknown(int notTheDsos)
        {
            return OffsetSymptomaticOnsetDateUnknown + notTheDsos;
        }

        public Range<int> ParseToRange(int value)
        {
            var intervalDuration = JavascriptMaths.Round(value / (double)RangeFactor); //TODO Check the intent?
            var daysSinceLastDayOfInterval = value - intervalDuration * RangeFactor;
            return new Range<int>(daysSinceLastDayOfInterval - (intervalDuration - 1), daysSinceLastDayOfInterval);
        }
        
        //From the JS
        public bool TryDecode(int value, out DsosDecodeResult? result)
        {
            result = null;

            if (ValidRange.Contains(value))
            {
                result = DsosDecodeResult.CreateSymptomatic(value);
                return true;
            }

            //1 day @ -14 to 19 days @ +21
            if (85 < value && value < 1922)
            {
                var intervalDuration = JavascriptMaths.Round(value / (double)RangeFactor); //TODO Check the intent?
                var daysSinceLastDayOfInterval = value - intervalDuration * RangeFactor;

                //TODO 
                //if (!ValidRange.Contains(daysSinceLastDayOfInterval))
                //    return false;

                var r =  new Range<int>(daysSinceLastDayOfInterval - (intervalDuration - 1), daysSinceLastDayOfInterval);

                result = DsosDecodeResult.CreateSymptomatic(r);
                return true;
            }

            if (ValidRange.Lo + OffsetSymptomaticOnsetDateUnknown <= value && value <= OffsetSymptomaticOnsetDateUnknown)
            {
                result = DsosDecodeResult.CreateSymptomaticOnsetUnknown(value - OffsetSymptomaticOnsetDateUnknown);
                return true;
            }

            if (value >= OffsetAsymptomatic + ValidRange.Lo && value <= OffsetAsymptomatic)
            {
                result = DsosDecodeResult.CreateAsymptomatic(value - OffsetAsymptomatic);
                return true;
            }

            if (value >= OffsetSymptomStateUnknown + ValidRange.Lo && value <= OffsetSymptomStateUnknown)
            {
                result = DsosDecodeResult.CreateSymptomsStatusUnknown(value - OffsetSymptomStateUnknown);
                return true;
            }

            result = null;
            return false;
        }

        public bool TryEncodeSymptomatic(int dsos, out int encodedResult)
        {
            var result = ValidRange.Contains(dsos);
            encodedResult = dsos;
            return result;
        }

        //public bool TryEncodeSymptomaticDateRange(int daysSinceLastDayOfRange, int rangeSpanDays, out int encodedResult)
        //{
        //    //TODO Check!
        //    var result = 0 < daysSinceLastDayOfRange && daysSinceLastDayOfRange < 14 && 0 < rangeSpanDays && rangeSpanDays < 14; //&& beginning of range?
        //    encodedResult = rangeSpanDays * RangeFactor + daysSinceLastDayOfRange;
        //    return result;
        //}

        //public bool TryEncodeSymptomaticOnsetUnknown(int daysSinceSubmission, out int encodedResult)
        //{
        //    var result = ValidRange(daysSinceSubmission);
        //    encodedResult = result ? daysSinceSubmission + OffsetSymptomaticOnsetDateUnknown : int.MinValue;
        //    return result;
        //}

        //public bool TryEncodeSymptomStatusUnknown(int daysSinceSubmission, out int encodedResult)
        //{
        //    var result = ValidRange(daysSinceSubmission);
        //    encodedResult = result ? daysSinceSubmission + OffsetSymptomStateUnknown : int.MinValue;
        //    return result;
        //}


        //public bool TryEncodeAsymptomatic(int daysSinceSubmission, out int encodedResult)
        //{
        //    var result = ValidRange(daysSinceSubmission);
        //    encodedResult = result ? daysSinceSubmission + OffsetAsymptomatic : int.MinValue;
        //    return result;
        //}

    }
}