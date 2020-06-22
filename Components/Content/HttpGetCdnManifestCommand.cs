// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content.NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Differs to the generic Cdn get as it will be different data on the same URI.
    /// </summary>
    public class HttpGetCdnManifestCommand
    {
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ExposureContentDbContext _ContentDb;

        public HttpGetCdnManifestCommand(IUtcDateTimeProvider dateTimeProvider, ExposureContentDbContext contentDb)
        {
            _DateTimeProvider = dateTimeProvider;
            _ContentDb = contentDb;
        }

        public async Task Execute(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400;
            }

            var now = _DateTimeProvider.Now();

            var content = _ContentDb.Set<ManifestEntity>()
            .Where(x => x.Release < now)
            .OrderByDescending(x => x.Release)
            .Take(1)
            .SingleOrDefault();

            if (content == null)
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.ContentLength = 0;
                return;
            }

            if (etagValue == content.PublishingId)
            {
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 304;
                return;
            }

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", content.SignedContentTypeName);
            httpContext.Response.Headers.Add("x-vws-signed", true.ToString());

            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = content.SignedContent.Length;
            await httpContext.Response.Body.WriteAsync(content.SignedContent);
        }
    }
}