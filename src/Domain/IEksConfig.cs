// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    
    
    public interface IEksConfig
    {

        /// <summary>
        /// Manifest Builder
        /// </summary>
        int LifetimeDays { get; }
        
        /// <summary>
        /// Max size
        /// </summary>
        int TekCountMax { get; }

        /// <summary>
        /// Stuffing
        /// </summary>
        int TekCountMin { get; }

        int PageSize { get; }

        bool CleanupDeletesData { get; }
    }
}
