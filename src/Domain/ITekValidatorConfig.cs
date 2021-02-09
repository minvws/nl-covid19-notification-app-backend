// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Domain
{
    public interface ITekValidatorConfig
    {
        /// <summary>
        /// Cannot currently align with IExposureKeySetsConfig.LifetimeDays.
        /// iOS sends 14 stored keys regardless of their date.
        /// Re-instate validation (as opposed to just filtering) this setting when iOS fixes the bug.
        /// </summary>
        int MaxAgeDays { get; }

        /// <summary>
        /// Anti-Spam = date of release of GA SDKs
        /// See MaxAgeDays
        /// </summary>
        int RollingStartNumberMin { get; }

        int PublishingDelayInMinutes { get; }
        int AuthorisationWindowMinutes { get; }
    }
}