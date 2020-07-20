// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public class IccPortalConfig : AppSettingsReader, IIccPortalConfig
    {
        public IccPortalConfig(IConfiguration config, string? prefix = null) : base(config, prefix)
        {
        }

        public string BaseUrl => GetConfigValue(nameof(BaseUrl), String.Empty);
        public string Tenant => GetConfigValue(nameof(Tenant), String.Empty);
        public string ClientId => GetConfigValue(nameof(ClientId), String.Empty);
        public string ClientSecret => GetConfigValue(nameof(ClientSecret), String.Empty);
        public string JwtSecret => GetConfigValue("Jwt:Secret", String.Empty);
        public double ClaimLifetimeHours => GetConfigValue(nameof(ClaimLifetimeHours), 3.0);
    }
}