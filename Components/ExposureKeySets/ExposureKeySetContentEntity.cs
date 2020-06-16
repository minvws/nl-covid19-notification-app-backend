// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
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
        /// </summary>
        public string CreatingJobName { get; set; }
        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public int CreatingJobQualifier { get; set; }
    }

    /// <summary>
    /// TODO incomplete.
    /// </summary>
    public class EksCreateJobOutputEntity
    {
        public int Id { get; set; }
        public DateTime Release { get; set; }
        public string Region { get; set; } = DefaultValues.Region;
        public byte[]? Content { get; set; }

        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public string CreatingJobName { get; set; }

        /// <summary>
        /// Metadata - Job Id.
        /// </summary>
        public int CreatingJobQualifier { get; set; }
    }
}