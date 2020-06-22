// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle
{
    public abstract class ContentEntity
    {
        [Key]
        public int Id { get; set; }
        public DateTime Release { get; set; }
        
        /// <summary>
        /// Publishing Id is f(Content)
        /// </summary>
        public string PublishingId { get; set; }
        public byte[]? Content { get; set; }
        public string? ContentTypeName { get; set; }
        public byte[]? SignedContent { get; set; }
        public string? SignedContentTypeName { get; set; }
    }
}