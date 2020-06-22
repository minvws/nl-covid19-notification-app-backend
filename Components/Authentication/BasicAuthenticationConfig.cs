// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Authentication
{
    public class BasicAuthenticationConfig : AppSettingsReader, IBasicAuthenticationConfig
    {
        public BasicAuthenticationConfig(IConfiguration config) : base(config, "Authentication")
        {
        }

        public string UserName => GetValue(nameof(UserName));
        public string Password => GetValue(nameof(Password));
    }
}
