// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Includes mitigations for CDN cache miss/stale item edge cases.
    /// No Accept headers required or processed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetSignedCdnContentOnlyCommand<T> where T: ContentEntity
    {
        private readonly IReader<T> _SafeReader;
        private readonly IPublishingId _PublishingId;
        private readonly ILogger _Logger;

        public HttpGetSignedCdnContentOnlyCommand(IReader<T> safeReader, IPublishingId publishingId, ILogger<HttpGetSignedCdnContentOnlyCommand<T>> logger)
        {
            _SafeReader = safeReader ?? throw new ArgumentNullException(nameof(safeReader));
            _PublishingId = publishingId ?? throw new ArgumentNullException(nameof(publishingId));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Execute(HttpContext httpContext, string id)
        {
            if (httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                _Logger.LogError($"Required request header missing - if-none-match.");
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400; //TODO!
            }

            if (typeof(T) != typeof(ManifestEntity) && !_PublishingId.Validate(id))
            {
                _Logger.LogError($"Invalid content id - {id}.");
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            var content = await _SafeReader.Execute(id);
            
            if (content == null)
            {
                //TODO tell CDN to ignore hunting?
                _Logger.LogError($"Content not found - {id}.");
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return;
            }

            if (etagValue == content.PublishingId)
            {
                _Logger.LogWarning($"Matching etag found, responding with 304 - {id}.");
                httpContext.Response.StatusCode = 304;
                httpContext.Response.ContentLength = 0;
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