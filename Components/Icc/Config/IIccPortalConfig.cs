// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Config
{
    public interface IIccPortalConfig
    {
        string FrontendBaseUrl { get; }
        string JwtSecret { get; }
        double ClaimLifetimeHours { get; }
        bool StrictRolePolicyEnabled { get; }
    }
}