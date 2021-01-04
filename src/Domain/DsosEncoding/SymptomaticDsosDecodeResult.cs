using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding
{
    public class SymptomaticDsosDecodeResult : DsosDecodeResult
    {
        public SymptomaticDsosDecodeResult(Range<int> dayRange) : base(SymptomObservation.Symptomatic, dayRange)
        {
            SymptomsOnsetDatePrecision = SymptomsOnsetDatePrecision.Range;
        }

        public SymptomaticDsosDecodeResult(int value, bool exact) : base(SymptomObservation.Symptomatic, new Range<int>(value))
        {
            SymptomsOnsetDatePrecision = exact ? SymptomsOnsetDatePrecision.Exact : SymptomsOnsetDatePrecision.Unknown;
        }

        public SymptomsOnsetDatePrecision SymptomsOnsetDatePrecision { get; }

        public Range<int> DaysSinceLastSymptoms => SymptomsOnsetDatePrecision == SymptomsOnsetDatePrecision.Range ? Values : throw new InvalidOperationException();
        public int DaysSinceOnsetOfSymptoms => SymptomsOnsetDatePrecision == SymptomsOnsetDatePrecision.Exact ? Values.Hi : throw new InvalidOperationException();
        public override int DaysSinceSubmission => SymptomsOnsetDatePrecision == SymptomsOnsetDatePrecision.Unknown ? Values.Hi : throw new InvalidOperationException();
    }
}