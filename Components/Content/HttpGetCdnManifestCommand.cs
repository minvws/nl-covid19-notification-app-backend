// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Differs to the generic Cdn get as it will be different data on the same URI.
    /// </summary>
    public class HttpGetCdnManifestCommand
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ContentDbContext _ContentDb;
        private readonly ManifestCacheControlHeaderProcessor _CacheControlHeaderProcessor;

        public HttpGetCdnManifestCommand(IUtcDateTimeProvider dateTimeProvider, ContentDbContext contentDb, ManifestCacheControlHeaderProcessor cacheControlHeaderProcessor)
        {
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ContentDb = contentDb ?? throw new ArgumentNullException(nameof(contentDb));
            _CacheControlHeaderProcessor = cacheControlHeaderProcessor ?? throw new ArgumentNullException(nameof(cacheControlHeaderProcessor));
        }

        public async Task Execute(HttpContext httpContext)
        {
            var e = await _ContentDb.SafeGetLatestContent(ContentTypes.Manifest, _DateTimeProvider.Snapshot);
            if (e == null)
            {
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return;
            }

            //if (etagValue == content.PublishingId)
            //{
            //    httpContext.Response.ContentLength = 0;
            //    httpContext.Response.StatusCode = 304;
            //    return;
            //}

            httpContext.Response.Headers.Add("etag", e.PublishingId);
            httpContext.Response.Headers.Add("last-modified", e.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", e.ContentTypeName);
            httpContext.Response.Headers.Add("x-vws-signed", true.ToString());

            _CacheControlHeaderProcessor.Execute(httpContext, e);

            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = e.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}