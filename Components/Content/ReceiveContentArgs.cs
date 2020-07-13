// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class ReceiveContentArgs
    {
        public string PublishingId { get; set; }
        public DateTime LastModified { get; set; }
        public byte[]? SignedContent { get; set; }
    }
}