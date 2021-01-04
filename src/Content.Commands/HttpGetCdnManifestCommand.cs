// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    /// <summary>
    /// Differs to the generic Cdn get as it will be different data on the same URI.
    /// </summary>
    public class HttpGetCdnManifestCommand
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IHttpResponseHeaderConfig _HttpResponseHeaderConfig;
        private readonly ContentDbContext _ContentDb;

        public HttpGetCdnManifestCommand(IUtcDateTimeProvider dateTimeProvider, IHttpResponseHeaderConfig httpResponseHeaderConfig, ContentDbContext contentDb)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _HttpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
            _ContentDb = contentDb ?? throw new ArgumentNullException(nameof(contentDb));
        }

        public async Task ExecuteAsync(HttpContext httpContext, string version)
        {

            var e = await _ContentDb.SafeGetLatestContentAsync(version, _DateTimeProvider.Snapshot);
            if (e == null)
            {
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return;
            }

            httpContext.Response.Headers.Add("etag", e.PublishingId);
            httpContext.Response.Headers.Add("last-modified", e.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", e.ContentTypeName);
            httpContext.Response.Headers.Add("cache-control", _HttpResponseHeaderConfig.ManifestCacheControl);
            httpContext.Response.Headers.Add("x-vws-signed", true.ToString());
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = e.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}