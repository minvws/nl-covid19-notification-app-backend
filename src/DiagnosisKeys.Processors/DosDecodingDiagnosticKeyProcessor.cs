using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class DosDecodingDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        public const string DecodedDsosMetadataKey = "DecodedDSOS";

        public DkProcessingItem? Execute(DkProcessingItem? value)
        {
            if (value == null) return value;

            if (!new DsosEncodingService().TryDecode(value.DiagnosisKey.Efgs.DaysSinceSymptomsOnset.Value, out var result))
            {
                //TODO log
                return null;
            }

            value.Metadata[DecodedDsosMetadataKey] = result;
            return value;
        }
    }
}