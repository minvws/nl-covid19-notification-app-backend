// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice
{
    public abstract class ContentEntity
    {
        public string PublishingId { get; set; }
        public DateTime Release { get; set; }
        public string Region { get; set; } = DefaultValues.Region;
        public byte[] Content { get; set; }
        public string ContentTypeName { get; set; }
    }
}