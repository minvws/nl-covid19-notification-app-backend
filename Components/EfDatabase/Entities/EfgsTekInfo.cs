// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities
{
    /// <summary>
    /// What was received from the EFGS
    /// Immutable except for Published
    /// </summary>
    [Owned]
    public class EfgsTekInfo
    {
        /// <summary>
        /// The batch it was imported from or published to.
        /// </summary>
        public string? BatchTag { get; set; }
        /// <summary>
        /// Comma-seperated list of 2 character country codes
        /// </summary>
        public string? CountriesOfInterest { get; set; }
        public TransmissionRiskLevel? TransmissionRiskLevel { get; set; }
        public int? DaysSinceSymptomsOnset { get; set; }
        public ReportType? ReportType { get; set; }
    }
}