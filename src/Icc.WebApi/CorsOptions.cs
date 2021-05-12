// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Config;

namespace NL.Rijksoverheid.ExposureNotification.Icc.WebApi
{
    public class CorsOptions : ICorsOptions
    {
        private readonly IIccPortalConfig _IccPortalConfig;

        /// <summary>
        /// Builds options for the CorsPolicyBuilder
        /// </summary>
        /// <param name="iccPortalConfig"></param>
        public CorsOptions(IIccPortalConfig iccPortalConfig)
        {
            _IccPortalConfig = iccPortalConfig ?? throw new ArgumentNullException(nameof(iccPortalConfig));
        }

        /// <summary>
        /// Builds options on the CorsPolicyBuilder parameter
        /// </summary>
        /// <param name="options"></param>
        public void Build(CorsPolicyBuilder options)
        {
            options.WithOrigins(GetOrigins()).WithMethods(GetMethods()).WithHeaders(GetHeaders());
        }

        private string[] GetHeaders()
        {
            return new[] {"Authorization","Content-Type"};
        }

        private string[] GetMethods()
        {
            return new[] {"POST", "PUT", "GET", "OPTIONS"};
        }
        
        private string[] GetOrigins()
        {
            var origin = new OriginBuilder(_IccPortalConfig.FrontendBaseUrl).GetOrigin();
            return new[] {origin};
        }

    }
}