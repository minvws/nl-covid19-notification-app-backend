// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Config
{
    public class IccPortalConfig : AppSettingsReader, IIccPortalConfig
    {
        public IccPortalConfig(IConfiguration config, string? prefix = "IccPortal") : base(config, prefix)
        {
        }

        public string JwtSecret => GetConfigValue("Jwt:Secret", string.Empty);
        public double ClaimLifetimeHours => GetConfigValue(nameof(ClaimLifetimeHours), 3.0);
        
        public string FrontendBaseUrl => GetConfigValue(nameof(FrontendBaseUrl), "Missed! Here are some nasty non-uri characters \"<>#%.");
        public bool StrictRolePolicyEnabled => GetConfigValue(nameof(StrictRolePolicyEnabled), true);
    }
}