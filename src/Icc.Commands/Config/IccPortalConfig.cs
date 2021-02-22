// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using System.Runtime.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class IccPortalConfig : AppSettingsReader, IIccPortalConfig
    {
        private static readonly DefaultProductionValuesIccPortalConfig _DefaultProductionValues = new DefaultProductionValuesIccPortalConfig();

        public IccPortalConfig(IConfiguration config, string prefix = "IccPortal") : base(config, prefix)
        {
        }

        public string JwtSecret => GetConfigValue<string>("Jwt:Secret");
        public double ClaimLifetimeHours => GetConfigValue(nameof(ClaimLifetimeHours), _DefaultProductionValues.ClaimLifetimeHours);
        
        public string FrontendBaseUrl => GetConfigValue<string>(nameof(FrontendBaseUrl));
        public string BackendBaseUrl => GetConfigValue<string>(nameof(BackendBaseUrl));
        public bool StrictRolePolicyEnabled => GetConfigValue(nameof(StrictRolePolicyEnabled), _DefaultProductionValues.StrictRolePolicyEnabled);
    }
}