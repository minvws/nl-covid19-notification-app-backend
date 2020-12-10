// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities
{
    /// <summary>
    /// AKA Interoperability Input
    /// </summary>
    [Table("IksCreateJobInput")]
    public class IksCreateJobInputEntity
    {
        /// <summary>
        /// This is the id for this table - taken from the original table - NOT an identity/hilo
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        //There is no stuffing here so all input had a DkId
        public long DkId { get; set; }

        /// <summary>
        /// Set when the exposure key set it written
        /// </summary>
        public bool Used { get; set; }
        public DailyKey DailyKey { get; set; }
        public TransmissionRiskLevel TransmissionRiskLevel { get; set; }
        public ReportType ReportType { get; set; }
        public string CountriesOfInterest { get; set; }
        public int DaysSinceSymptomsOnset { get; set; }
    }
}