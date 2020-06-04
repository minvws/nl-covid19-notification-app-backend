// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public abstract class ContentEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string PublishingId { get; set; }
        [Index]
        public DateTime Release { get; set; }
        [Index]
        public string Region { get; set; } = DefaultValues.Region;
        public byte[] Content { get; set; }
        public string ContentTypeName { get; set; } = "application/json"; //TODO
    }
}