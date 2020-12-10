// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    //Straight mapping
    public class DateOfSymptomsOnsetDiagnosticKeyProcessor : IDiagnosticKeyProcessor
    {
        public DkProcessingItem? Execute(DkProcessingItem? value)
        {
            if(value == null) return value;

            value.DiagnosisKey.Local.DaysSinceSymptomsOnset = value.DiagnosisKey.Efgs.DaysSinceSymptomsOnset;

            return value;
        }
    }
}