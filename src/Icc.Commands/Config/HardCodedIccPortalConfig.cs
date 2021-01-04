// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class HardCodedIccPortalConfig : IIccPortalConfig
    {
        public HardCodedIccPortalConfig(IIccIdentityHubConfig? identityHubConfig, string frontendBaseUrl, string jwtSecret, double claimLifetimeHours, bool strictRolePolicyEnabled)
        {
            IdentityHubConfig = identityHubConfig;
            FrontendBaseUrl = frontendBaseUrl;
            JwtSecret = jwtSecret;
            ClaimLifetimeHours = claimLifetimeHours;
            StrictRolePolicyEnabled = strictRolePolicyEnabled;
        }

        public IIccIdentityHubConfig IdentityHubConfig { get; }
        public string FrontendBaseUrl { get; }
        public string JwtSecret { get; }
        public double ClaimLifetimeHours { get; }
        public bool StrictRolePolicyEnabled { get; }
    }
}