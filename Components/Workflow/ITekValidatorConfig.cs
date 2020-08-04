// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow
{
    public interface ITekValidatorConfig
    {
        /// <summary>
        /// Align with IExposureKeySetsConfig.LifetimeDays
        /// </summary>
        int MaxAgeDays { get; }
        int KeyDataLength { get; }
        int RollingPeriodMin { get; }
        int RollingPeriodMax { get; }
        
        /// <summary>
        /// Effectively a date -> date of go live
        /// </summary>
        int RollingStartNumberMin { get; }

        int PublishingDelayInMinutes { get; }
        int AuthorisationWindowMinutes { get; }
    }
}