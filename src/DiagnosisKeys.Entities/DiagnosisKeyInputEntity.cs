// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities
{
    /// <summary>
    /// Job table for importing TEKs
    /// </summary>
    [Table(TableNames.DiagnosisKeysInput)]
    public class DiagnosisKeyInputEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public long TekId { get; set; }
        public DailyKey DailyKey { get; set; } = new DailyKey();
        public LocalTekInfo Local { get; set; } = new LocalTekInfo();
    }
}