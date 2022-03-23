// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Entities
{
    public class DashboardInputJsonEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public DateTime? DownloadedDate { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public string Hash { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string JsonData { get; set; }
    }
}
