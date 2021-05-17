// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain.Rcp;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities
{
    [Table(TableNames.EksEngineInput)]
    public class EksCreateJobInputEntity
    {
        /// <summary>
        /// This is the id for this table - taken from the original table - NOT an identity/hilo
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? TekId { get; set; }

        /// <summary>
        /// Set when the exposure key set it written
        /// </summary>
        public bool Used { get; set; }
        public byte[] KeyData { get; set; }
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }
        public TransmissionRiskLevel TransmissionRiskLevel { get; set; }
        public int DaysSinceSymptomsOnset { get; set; }
        public InfectiousPeriodType Symptomatic { get; set; }
        public int ReportType { get; set; }
    }
}