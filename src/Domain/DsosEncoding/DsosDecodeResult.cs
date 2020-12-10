using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding
{
    public class DsosDecodeResult
    {
        protected Range<int> Values { get; }
        public SymptomObservation SymptomObservation { get; }
        public virtual int DaysSinceSubmission => SymptomObservation != SymptomObservation.Symptomatic ? Values.Hi : throw new InvalidOperationException();

        protected DsosDecodeResult(SymptomObservation symptomObservation, Range<int> values)
        {
            //TODO ranges break this contraint.
            //if (!DsosService.ValidRange.Contains(values))
            //    throw new ArgumentOutOfRangeException(nameof(values));

            SymptomObservation = symptomObservation;
            Values = values;
        }

        public static DsosDecodeResult CreateAsymptomatic(int daysSinceSubmission) => new DsosDecodeResult(SymptomObservation.Asymptomatic, new Range<int>(daysSinceSubmission));
        public static DsosDecodeResult CreateSymptomsStatusUnknown(int daysSinceSubmission) => new DsosDecodeResult(SymptomObservation.Unknown, new Range<int>(daysSinceSubmission));
        public static DsosDecodeResult CreateSymptomatic(Range<int> relativeDayRange) => new SymptomaticDsosDecodeResult(relativeDayRange);
        public static DsosDecodeResult CreateSymptomatic(int daysSinceOnsetOfSymptoms) => new SymptomaticDsosDecodeResult(daysSinceOnsetOfSymptoms, true);
        public static DsosDecodeResult CreateSymptomaticOnsetUnknown(int daysSinceSubmission) => new SymptomaticDsosDecodeResult(daysSinceSubmission, false);
        public SymptomaticDsosDecodeResult AsSymptomatic() => this as SymptomaticDsosDecodeResult ?? throw new InvalidOperationException();
    }
}
