// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public interface IIccPortalConfig
    {
        
        IIccIdentityHubConfig IdentityHubConfig { get; }
        string FrontendBaseUrl { get; }
        string JwtSecret { get; }
        double ClaimLifetimeHours { get; }
        
        
    }
}