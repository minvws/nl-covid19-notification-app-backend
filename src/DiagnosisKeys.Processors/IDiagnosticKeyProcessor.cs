// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Processors
{
    /// <summary>
    /// Implementation of ingress and egress filters.
    /// Map values from Efgs to TekSource.LocalTekInfo.
    /// Return null to exclude.
    /// </summary>
    public interface IDiagnosticKeyProcessor
    {
        DkProcessingItem? Execute(DkProcessingItem? value);
    }
}