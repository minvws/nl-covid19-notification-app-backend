// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Security.Policy;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend
{
    public class CORSOptions
    {
        private readonly IccPortalConfig _IccPortalConfig;

        /// <summary>
        /// Builds options for the CorsPolicyBuilder
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="iccPortalConfig"></param>
        public CORSOptions(IccPortalConfig iccPortalConfig)
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
            return new[] {"Authorization"};
        }

        private string[] GetMethods()
        {
            return new[] {"POST", "GET", "OPTIONS"};
        }
        
        private string[] GetOrigins()
        {
            var origin = new OriginBuilder(_IccPortalConfig.FrontendBaseUrl).getOrigin();
            return new[] {origin};
        }

    }
}