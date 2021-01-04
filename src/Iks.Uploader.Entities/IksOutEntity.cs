// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities
{
    /// <summary>
    /// AKA Interop Content
    /// </summary>
    [Table("IksOut")]
    public class IksOutEntity
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }

        //Date used to derive the DaysSinceInfected for DKs
        public DateTime ValidFor { get; set; }
        public byte[] Content { get; set; }
        public bool Sent { get; set; }

        /// <summary>
        /// TODO CreatingJobQualifier?
        /// </summary>
        public int Qualifier { get; set; }

        public bool Error { get; set; }
    }
}