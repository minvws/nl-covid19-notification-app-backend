// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class NlSymptomaticFromDecodedDosDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        public DkProcessingItem Execute(DkProcessingItem value)
        {
            if (value == null)
            {
                return value;
            }

            if (!value.Metadata.TryGetValue(DosDecodingDiagnosticKeyProcessor.DecodedDsosMetadataKey, out var decodedDos))
            {
                return null;
            }

            // decodedDos map to InfectiousPeriodType
            value.DiagnosisKey.Local.Symptomatic = GetSymptomObservation((DsosDecodeResult)decodedDos);

            return value;
        }

        private InfectiousPeriodType GetSymptomObservation(DsosDecodeResult value)
        {
            return value.SymptomObservation == SymptomObservation.Asymptomatic
                ? InfectiousPeriodType.Asymptomatic
                : InfectiousPeriodType.Symptomatic; // Defaults to Symptomatic
        }
    }
}
