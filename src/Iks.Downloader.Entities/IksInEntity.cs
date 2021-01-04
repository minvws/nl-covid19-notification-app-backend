// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Downloader.Entities
{
    [Table("IksIn")]
    public class IksInEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string BatchTag { get; set; }
        public DateTime Created { get; set; }
        public byte[] Content { get; set; }
        //public IksInJobEntity? Received { get; set; }

        /// <summary>
        /// DKs have been processed and are in TekSource
        /// </summary>
        public DateTime? Accepted { get; set; }

        public bool Error { get; set; }
    }
}