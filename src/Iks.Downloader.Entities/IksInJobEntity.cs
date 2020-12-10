// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities
{

    //1 row only
    [Table("IksInJob")]
    public class IksInJobEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Cannot be NOT NULLABLE
        public string LastBatchTag { get; set; } = string.Empty;

        public DateTime LastRun { get; set; } = DateTime.MinValue;
    }
}