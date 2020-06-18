// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine
{

    public class EksCreateJobInputEntity
    {
        /// <summary>
        /// This is the id for this table - taken from the original table - NOT an identity/hilo
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Set when the exposure key set it written
        /// </summary>
        public bool Used { get; set; }

        public byte[] KeyData { get; set; }
        public int RollingStartNumber { get; set; }
        public int RollingPeriod { get; set; }
        public int TransmissionRiskLevel { get; set; }
    }
}