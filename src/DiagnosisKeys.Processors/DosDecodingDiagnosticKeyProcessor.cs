// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.DsosEncoding;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class DosDecodingDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        public const string DecodedDsosMetadataKey = "DecodedDSOS";

        public DkProcessingItem Execute(DkProcessingItem value)
        {
            if (value == null)
                return value;

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
