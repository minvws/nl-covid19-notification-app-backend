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
        private readonly Dictionary<string, string> _cdnBaseUrls;
        private readonly Dictionary<string, string> _appBaseUrls;
        private readonly Dictionary<string, string> _iccApiBaseUrls;
        private readonly Dictionary<string, string> _iccPortalBaseUrls;
        private readonly Dictionary<string, string> _cdnBaseUrlsInternal;
        private readonly Dictionary<string, string> _appBaseUrlsInternal;
        private readonly Dictionary<string, string> _iccApiBaseUrlsInternal;
        private readonly Dictionary<string, string> _iccPortalBaseUrlsInternal;

        public EndpointConfig(IConfiguration config, string prefix = null) : base(config, prefix)
        {
            _cdnBaseUrls = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5001"},
                {"test", "https://test.coronamelder-dist.nl"},
                {"acc", "https://acceptatie.coronamelder-dist.nl"},
                {"prod", "https://productie.coronamelder-dist.nl"}
            };

            _appBaseUrls = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5002"},
                {"test", "https://test.coronamelder-api.nl"},
                {"acc", "https://acceptatie.coronamelder-api.nl"},
                {"prod", "https://coronamelder-api.nl"}
            };

            _iccApiBaseUrls = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5003"},
                {"test", "https://test.coronamelder-portal.nl"},
                {"acc", "https://acceptatie.coronamelder-portal.nl"},
            };

            _iccPortalBaseUrls = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5011"},
                {"test", "https://test.coronamelder-portal.nl"},
                {"acc", "https://acceptatie.coronamelder-portal.nl"}
            };

            _cdnBaseUrlsInternal = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5001"},
                {"test", "http://corona-dist-content-DEV.covt.mhscibg.nl"},
                {"acc", "https://acceptatie.coronamelder-dist.nl"},
                {"prod", "https://productie.coronamelder-dist.nl"}
            };

            _appBaseUrlsInternal = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5002"},
                {"test", "http://corona-app-mobile-dev.covt.mhscibg.nl"},
                {"acc", "https://acceptatie.coronamelder-api.nl"},
                {"prod", "https://coronamelder-api.nl"}
            };

            _iccApiBaseUrlsInternal = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5003"},
                {"test", "http://corona-app-services-dev.covt.mhscibg.nl"},
                {"acc", "https://acceptatie.coronamelder-portal.nl"},
            };

            _iccPortalBaseUrlsInternal = new Dictionary<string, string>
            {
                {"dev", "https://localhost:5011"},
                {"test", "https://test.coronamelder-portal.nl"},
                {"acc", "https://acceptatie.coronamelder-portal.nl"}
            };
        }

        public string CdnBaseUrl(string environment, bool useInternal = false)
        {
            if (useInternal)
            {
                return _cdnBaseUrlsInternal[environment];
            }

            return _cdnBaseUrls[environment];
        }

        public string AppBaseUrl(string environment, bool useInternal = false)
        {
            if (useInternal)
            {
                return _appBaseUrlsInternal[environment];
            }

            return _appBaseUrls[environment];
        }

        public string IccApiBaseUrl(string environment, bool useInternal = false)
        {
            if (useInternal)
            {
                return _iccApiBaseUrlsInternal[environment];
            }

            return _iccApiBaseUrls[environment];
        }

        public string IccPortalBaseUrl(string environment, bool useInternal = false)
        {
            if (useInternal)
            {
                return _iccPortalBaseUrlsInternal[environment];
            }

            return _iccPortalBaseUrls[environment];
        }

        public string ManifestEndPoint => "manifest";

        public string AppConfigEndPoint => "appconfig";

        public string RiskCalculationParametersEndPoint => "riskcalculationparameters";

        public string ExposureKeySetEndPoint => "exposurekeyset";

        public string ResourceBundleEndPoint => "resourceBundle";

        public string RegisterEndPoint => "register";
    }
}
