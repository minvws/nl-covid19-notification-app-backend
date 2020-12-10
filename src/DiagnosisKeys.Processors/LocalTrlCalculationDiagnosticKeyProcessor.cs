// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    public class LocalTrlCalculationDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        public DkProcessingItem? Execute(DkProcessingItem? value)
        {
            if(value == null) return value;
            if(value.DiagnosisKey.Local.DaysSinceSymptomsOnset == null) return value;

            value.DiagnosisKey.Local.TransmissionRiskLevel = new TransmissionRiskLevelCalculationMk2().Calculate(value.DiagnosisKey.Local.DaysSinceSymptomsOnset.Value);
            
            return value;
        }
    }
}
