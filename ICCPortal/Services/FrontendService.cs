// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Configuration;

namespace NL.Rijksoverheid.ExposureNotification.IccPortalAuthorizer.Services
{
    public class FrontendService
    {
        private IConfiguration _Configuration;

        public FrontendService(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public string GetFrontendLoginUrl(string endpoint = "")
        {
            var frontendHost = _Configuration.GetSection("IccPortalConfig:FrontendHost").Value;
            var frontendRedirectUrl = "http://" + frontendHost + endpoint;
            return frontendRedirectUrl;
        }

        public string RedirectSuccesfullLogin(string jwtToken)
        {
            return GetFrontendLoginUrl("/auth?token=" + jwtToken);
        }
    }
}