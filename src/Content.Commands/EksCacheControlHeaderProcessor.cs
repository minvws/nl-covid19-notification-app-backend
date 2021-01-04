// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class EksCacheControlHeaderProcessor
    {
        private readonly EksMaxageCalculator _EksTtlCalculator;

        public EksCacheControlHeaderProcessor(EksMaxageCalculator eksTtlCalculator)
        {
            _EksTtlCalculator = eksTtlCalculator ?? throw new ArgumentNullException(nameof(eksTtlCalculator));
        }

        public void Execute(HttpContext httpContext, ContentEntity content)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (content == null) throw new ArgumentNullException(nameof(content));

            var ttl = _EksTtlCalculator.Execute(content.Created);
            httpContext.Response.Headers.Add("cache-control", $"public, immutable, max-age={ ttl }, s-maxage={ ttl }");
        }
    }
}