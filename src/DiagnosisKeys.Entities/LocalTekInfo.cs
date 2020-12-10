// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities
{
    /// <summary>
    /// Info from standard info 
    /// </summary>
    [Owned]
    public class LocalTekInfo
    {
        public TransmissionRiskLevel? TransmissionRiskLevel { get; set; }

        /// <summary>
        /// Set this from Days since symptoms onset, calculate TRL
        /// </summary>
        public int? DaysSinceSymptomsOnset { get; set; }
    }
}