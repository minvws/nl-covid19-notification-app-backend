// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public interface IIccPortalConfig
    {
        string BaseUrl { get; }
        string Tenant { get; }
        string ClientId { get; }
        string ClientSecret { get; }
        string JwtSecret { get; }
        double ClaimLifetimeHours { get; }
    }
}