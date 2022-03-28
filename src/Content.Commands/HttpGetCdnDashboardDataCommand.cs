// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public class HttpGetCdnDashboardDataCommand
    {
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly IHttpResponseHeaderConfig _httpResponseHeaderConfig;
        private readonly ContentDbContext _contentDb;

        public HttpGetCdnDashboardDataCommand(IUtcDateTimeProvider dateTimeProvider, IHttpResponseHeaderConfig httpResponseHeaderConfig, ContentDbContext contentDb)
        {
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _httpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
            _contentDb = contentDb ?? throw new ArgumentNullException(nameof(contentDb));
        }

        public async Task ExecuteAsync(HttpContext httpContext, ContentTypes type)
        {
            var e = await _contentDb.SafeGetLatestContentAsync(type, _dateTimeProvider.Snapshot);
            if (e == null)
            {
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return;
            }

            httpContext.Response.Headers.Add("etag", e.PublishingId);
            httpContext.Response.Headers.Add("last-modified", e.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", e.ContentTypeName);
            httpContext.Response.Headers.Add("cache-control", _httpResponseHeaderConfig.DashboardDataCacheControl);

            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = e.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            await httpContext.Response.Body.WriteAsync(e.Content);
        }
    }
}
