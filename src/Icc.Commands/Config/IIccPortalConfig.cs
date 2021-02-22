// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public interface IIccPortalConfig
    {
        /// <summary>
        /// Used in Icc.WebApi only.
        /// </summary>
        string FrontendBaseUrl { get; }

        /// <summary>
        /// Used in Icc.WebApi only.
        /// </summary>
        string BackendBaseUrl { get; }

        /// <summary>
        /// Used in Icc.Commands only.
        /// </summary>
        string JwtSecret { get; }

        /// <summary>
        /// Used in Icc.Commands only.
        /// </summary>
        double ClaimLifetimeHours { get; }
        
        /// <summary>
        /// Used in Icc.WebApi only.
        /// </summary>
        bool StrictRolePolicyEnabled { get; }
    }
}