// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public interface IEksConfig
    {
        /// <summary>
        /// Manifest Builder
        /// </summary>
        int LifetimeDays { get; }
        
        /// <summary>
        /// Max size.
        /// </summary>
        int TekCountMax { get; }

        /// <summary>
        /// EKS below this will be stuffed.
        /// </summary>
        int TekCountMin { get; }

        /// <summary>
        /// PageSize while reading TEKs from input table into memory when creating EKS.
        /// NB. There is no value set for this in deployment pipeline.
        /// </summary>
        int PageSize { get; }

        [Obsolete("Cleanup should always delete data.")]
        bool CleanupDeletesData { get; }
    }
}
