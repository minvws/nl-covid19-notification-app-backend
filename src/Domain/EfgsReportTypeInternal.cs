// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    
    /// <summary>
    /// Identical to Efgs type
    /// </summary>
    public enum EfgsReportTypeInternal
    {
        Unknown = 0,
        ConfirmedTest = 1,
        ConfirmedClinicalDiagnosis = 2,
        SelfReport = 3,
        Recursive = 4,
        Revoked = 5,
    }
}