// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Helpers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ContentApi
{
    public class CorsOptions : ICorsOptions
    {
        private readonly IWebHostEnvironment _Environment;

        public CorsOptions(IWebHostEnvironment environment)
        {
            _Environment = environment ?? throw new ArgumentNullException(nameof(environment));
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
                return new[]
                {
                    "http://localhost:5001", 
                    "https://test.coronamelder-dist.nl"
                };
            }
            else if (_Environment.IsStaging())
            {
                return new[] {"https://acceptatie.coronamelder-dist.nl"};
            }

            return new[] {"https://productie.coronamelder-dist.nl"};
        }
    }
}