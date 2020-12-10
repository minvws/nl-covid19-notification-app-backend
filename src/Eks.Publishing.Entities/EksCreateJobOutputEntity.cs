// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities
{
    [Table(TableNames.EksEngineOutput)]
    public class EksCreateJobOutputEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Release { get; set; }
        public string Region { get; set; } = DefaultValues.Region;
        public byte[] Content { get; set; }

        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public string CreatingJobName { get; set; } = "Deprecated";

        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public int CreatingJobQualifier { get; set; }
    }
}