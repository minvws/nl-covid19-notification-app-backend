// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace Core.E2ETests
{
    public class EndpointConfig : AppSettingsReader
    {
        private const string DefaultEnvironment = "dev";

        public EndpointConfig(IConfiguration config, string prefix = null) : base(config, prefix)
        {
        }

        public string Environment => GetConfigValue("environment", DefaultEnvironment);

        public string CdnBaseUrl => GetConfigValue($"baseUrls:cdn:{Environment}", "");

        public string ManifestEndPoint => GetConfigValue($"endpoints:cdn:manifest", "");

        public string AppConfigEndPoint => GetConfigValue($"endpoints:cdn:appConfig", "");

        public string RiskCalculationParametersEndPoint => GetConfigValue($"endpoints:cdn:riskCalculationParameters", "");

        public string ExposureKeySetEndPoint => GetConfigValue($"endpoints:cdn:exposureKeySet", "");

        public string ResourceBundleEndPoint => GetConfigValue($"endpoints:cdn:resourceBundle", "");
    }
}
