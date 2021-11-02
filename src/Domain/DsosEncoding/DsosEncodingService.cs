// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

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

        //From the JS
        public bool TryDecode(int value, out DsosDecodeResult result)
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
                var intervalDuration = JavascriptMaths.Round(value / (double)RangeFactor);
                var daysSinceLastDayOfInterval = value - intervalDuration * RangeFactor;

                var r = new Range<int>(daysSinceLastDayOfInterval - (intervalDuration - 1), daysSinceLastDayOfInterval);

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
    }
}
