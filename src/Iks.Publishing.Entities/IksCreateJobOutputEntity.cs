// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Publishing.Entities
{
    [Table("IksCreateJobOutput")]
    public class IksCreateJobOutputEntity
    {
        /// <summary>
        /// This is the id for this table - taken from the original table - NOT an identity/hilo
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime Created { get; set; }
        public byte[] Content { get; set; }
        public int CreatingJobQualifier { get; set; }
    }
}