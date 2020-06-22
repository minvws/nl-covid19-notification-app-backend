// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
{
    /// <summary>
    /// TODO incomplete.
    /// </summary>
    public class ExposureKeySetContentEntity : ContentEntity
    {
        /// <summary>
        /// Metadata - Job Id.
        /// Null in the CDN db
        /// </summary>
        public string? CreatingJobName { get; set; }
        /// <summary>
        /// Metadata - Job Id.
        /// Null in the CDN db
        /// </summary>
        public int? CreatingJobQualifier { get; set; }
    }
}