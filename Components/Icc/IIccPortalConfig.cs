// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public interface IIccPortalConfig
    {
        
        IIccIdentityHubConfig IdentityHubConfig { get; }
        string FrontendBaseUrl { get; }
        string JwtSecret { get; }
        double ClaimLifetimeHours { get; }
        
        
    }
}