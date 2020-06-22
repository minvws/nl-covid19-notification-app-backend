// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Includes mitigations for CDN cache miss/stale item edge cases.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetCdnContentCommand<T> where T: ContentEntity
    {
        private readonly IReader<T> _SafeReader;
        private readonly IPublishingId _PublishingId;

        public HttpGetCdnContentCommand(IReader<T> safeReader, IPublishingId publishingId)
        {
            _SafeReader = safeReader;
            _PublishingId = publishingId;
        }

        public async Task Execute(HttpContext httpContext, string id)
        {
            if (httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400; //TODO!
            }

            var parsed = _PublishingId.ParseUri(id);
            if (typeof(T) != typeof(ManifestEntity) && !_PublishingId.Validate(id))
            {
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            var content = await _SafeReader.Execute(parsed);
            
            if (content == null)
            {
                //TODO tell CDN to ignore hunting?
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return;
            }

            if (etagValue == content.PublishingId)
            {
                httpContext.Response.StatusCode = 304;
                httpContext.Response.ContentLength = 0;
                return;
            }

            var accepts = httpContext.Request.Headers["accept"].ToHashSet();

            var signedResponse = content.SignedContentTypeName != null && accepts.Contains(content.SignedContentTypeName);

            if (!signedResponse && !accepts.Contains(content.ContentTypeName))
            {
                httpContext.Response.StatusCode = 406;
                httpContext.Response.ContentLength = 0;
                return;
            }

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", signedResponse ? content.SignedContentTypeName : content.ContentTypeName);
            httpContext.Response.Headers.Add("x-vws-signed", signedResponse.ToString());

            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = (signedResponse ? content.SignedContent : content.Content).Length;
            await httpContext.Response.Body.WriteAsync(signedResponse ? content.SignedContent : content.Content);
        }
    }
}