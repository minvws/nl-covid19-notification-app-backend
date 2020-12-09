// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Helpers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi
{
    public class CorsOptions : ICorsOptions
    {
        private readonly IWebHostEnvironment _Environment;
        private readonly ContentApiConfig _Config;

        public CorsOptions(IWebHostEnvironment environment, ContentApiConfig config)
        {
            _Environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Build(CorsPolicyBuilder options)
        {
            options.WithMethods("GET").WithOrigins(GetEnvOrigins());
        }

        private string[] GetEnvOrigins()
        {
            // Swaggering via these urls
            if (_Environment.IsDevelopment())
            {
                var originBuilder = new OriginBuilder(_Config.Url);
                return new[]
                {
                    originBuilder.getOrigin()
                };
            }

            return new[] {""}; // Denies Swagger on acceptatie and productie 
        }
    }
}