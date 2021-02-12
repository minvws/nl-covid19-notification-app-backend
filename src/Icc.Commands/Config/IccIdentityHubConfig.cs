// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class IccIdentityHubConfig : AppSettingsReader, IIccIdentityHubConfig
    {
        private static readonly DefaultProductionValuesIccIdentityHubConfig _ProductionValues = new DefaultProductionValuesIccIdentityHubConfig();

        public IccIdentityHubConfig(IConfiguration config, string prefix = "IccPortal:IdentityHub") : base(config, prefix)
        {
        }

        public string BaseUrl => GetConfigValue(nameof(BaseUrl), _ProductionValues.BaseUrl);
        public string Tenant => GetConfigValue(nameof(Tenant), _ProductionValues.Tenant);
        public string ClientId => GetConfigValue(nameof(ClientId), _ProductionValues.ClientId);
        public string ClientSecret => GetConfigValue(nameof(ClientSecret), _ProductionValues.ClientSecret);
        public string CallbackPath => GetConfigValue(nameof(CallbackPath), _ProductionValues.CallbackPath);
    }
}