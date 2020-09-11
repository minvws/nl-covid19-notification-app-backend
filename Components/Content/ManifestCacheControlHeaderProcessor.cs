// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class ManifestCacheControlHeaderProcessor
    {
        private readonly ManifestMaxAgeCalculator _TTlCalculator;

        public ManifestCacheControlHeaderProcessor(ManifestMaxAgeCalculator tTlCalculator)
        {
            _TTlCalculator = tTlCalculator ?? throw new ArgumentNullException(nameof(tTlCalculator));
        }

        public void Execute(HttpContext httpContext, ContentEntity content)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var ttl = _TTlCalculator.Execute(content.Created);
            httpContext.Response.Headers.Add("cache-control", $"public, max-age={ ttl }, s-maxage={ ttl }");
        }
    }
}