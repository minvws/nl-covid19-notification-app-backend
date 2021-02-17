// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config
{
    public class IccIdentityHubConfig : AppSettingsReader, IIccIdentityHubConfig
    {
        //NB All of these values must be specified in the config file.

        public IccIdentityHubConfig(IConfiguration config, string prefix = "IccPortal:IdentityHub") : base(config, prefix)
        {
        }

        public string BaseUrl => GetConfigValue<string>(nameof(BaseUrl));
        public string Tenant => GetConfigValue<string>(nameof(Tenant));
        public string ClientId => GetConfigValue<string>(nameof(ClientId));
        public string ClientSecret => GetConfigValue<string>(nameof(ClientSecret));
        public string CallbackPath => GetConfigValue<string>(nameof(CallbackPath));
    }
}