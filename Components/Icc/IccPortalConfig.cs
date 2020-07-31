// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc
{
    public class IccPortalConfig : AppSettingsReader, IIccPortalConfig
    {
        public IccPortalConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
            IdentityHubConfig = new IccIdentityHubConfig(config, "IccPortalConfig:IdentityHub");
        }

        public IIccIdentityHubConfig IdentityHubConfig { get; }
        public string JwtSecret => GetConfigValue("Jwt:Secret", String.Empty);
        public double ClaimLifetimeHours => GetConfigValue(nameof(ClaimLifetimeHours), 3.0);
        
        public string FrontendBaseUrl => GetConfigValue(nameof(FrontendBaseUrl), "TODO Sensible default!!!!");
    }
}