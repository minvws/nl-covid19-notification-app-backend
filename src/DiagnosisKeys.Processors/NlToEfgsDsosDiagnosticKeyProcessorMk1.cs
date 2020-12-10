using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    /// <summary>
    /// All assumed to be symptomatic before 2020/12/01
    /// Dependent on the NL TRL calculation.
    /// </summary>
    public class NlToEfgsDsosDiagnosticKeyProcessorMk1 : IDiagnosticKeyProcessor
    {
        public DkProcessingItem? Execute(DkProcessingItem? value)
        {
            if (!value.DiagnosisKey.Local.TransmissionRiskLevel.HasValue || !value.DiagnosisKey.Local.DaysSinceSymptomsOnset.HasValue)
                throw new InvalidOperationException($"{nameof(NlToEfgsDsosDiagnosticKeyProcessorMk1)} requires TRL and DaysSinceSymptomsOnset.");

            var baseValue = new DosViaTrlDayRangeMidPointCalculation().Calculate(value.DiagnosisKey.Local.DaysSinceSymptomsOnset.Value);
            value.DiagnosisKey.Efgs.DaysSinceSymptomsOnset = new DsosEncodingService().EncodeSymptomaticOnsetDateUnknown(baseValue);
            return value;
        }
    }
}