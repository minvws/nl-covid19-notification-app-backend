// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using System.Runtime.Serialization;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class DefaultProductionValuesIccPortalConfig : IIccPortalConfig
    {
        public string FrontendBaseUrl => throw new MissingConfigurationValueException(nameof(FrontendBaseUrl));
        public string JwtSecret => throw new MissingConfigurationValueException(nameof(JwtSecret));
        public double ClaimLifetimeHours => 3.0;
        public bool StrictRolePolicyEnabled => true;
    }

    public class IccPortalConfig : AppSettingsReader, IIccPortalConfig
    {
        private static readonly DefaultProductionValuesIccPortalConfig _DefaultProductionValues = new DefaultProductionValuesIccPortalConfig();

        public IccPortalConfig(IConfiguration config, string prefix = "IccPortal") : base(config, prefix)
        {
        }

        public string JwtSecret => GetConfigValue("Jwt:Secret", _DefaultProductionValues.JwtSecret);
        public double ClaimLifetimeHours => GetConfigValue(nameof(ClaimLifetimeHours), _DefaultProductionValues.ClaimLifetimeHours);
        
        public string FrontendBaseUrl => GetConfigValue(nameof(FrontendBaseUrl), _DefaultProductionValues.FrontendBaseUrl);
        public bool StrictRolePolicyEnabled => GetConfigValue(nameof(StrictRolePolicyEnabled), _DefaultProductionValues.StrictRolePolicyEnabled);
    }
}