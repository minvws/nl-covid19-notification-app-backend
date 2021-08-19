// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace Core.E2ETests
{
    public class EndpointConfig : AppSettingsReader
    {
        private readonly Dictionary<string, string> _endpoints;

        public EndpointConfig(IConfiguration config, string prefix = null) : base(config, prefix)
        {
            _endpoints = new Dictionary<string, string>
            {
                {"dev", "http://localhost:5004"},
                {"test", "https://test.coronamelder-dist.nl"},
                {"acc", "https://acceptatie.coronamelder-dist.nl"},
                {"prod", "https://productie.coronamelder-dist.nl"}
            };
        }
        
        public string CdnBaseUrl(string environment)
        {
            return _endpoints[environment];
        }

        public string ManifestEndPoint => "manifest";

        public string AppConfigEndPoint => "appconfig";

        public string RiskCalculationParametersEndPoint => "riskcalculationparameters";

        public string ExposureKeySetEndPoint => "exposurekeyset";

        public string ResourceBundleEndPoint => "resourceBundle";
    }
}
