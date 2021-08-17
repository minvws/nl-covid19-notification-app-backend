// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;

namespace Core.E2ETests
{
    public class TestBase
    {
        public IConfigurationRoot ConfigurationRoot { get; }
        private string _environment = "dev";


        public TestBase()
        {
            ConfigurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()  // <== this is important
                .Build();

            _environment = ConfigurationRoot["environment"];
        }

        public string CdnBaseUrl
        {
            get
            {
                return ConfigurationRoot[$"baseUrls:cdn:{_environment}"];
            }
        }

        public string ManifestEndPoint
        {
            get
            {
                return ConfigurationRoot[$"endpoints:cdn:manifest"];
            }
        }

        public string AppConfigEndPoint
        {
            get
            {
                return ConfigurationRoot[$"endpoints:cdn:appConfig"];
            }
        }

        public string RiskCalculationParametersEndPoint
        {
            get
            {
                return ConfigurationRoot[$"endpoints:cdn:riskCalculationParameters"];
            }
        }

        public string ExposureKeySetEndPoint
        {
            get
            {
                return ConfigurationRoot[$"endpoints:cdn:exposureKeySet"];
            }
        }

        public string ResourceBundleEndPoint
        {
            get
            {
                return ConfigurationRoot[$"endpoints:cdn:resourceBundle"];
            }
        }
    }
}
