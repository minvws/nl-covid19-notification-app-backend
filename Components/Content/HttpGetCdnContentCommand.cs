// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Includes mitigations for CDN cache miss/stale item edge cases.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetCdnContentCommand
    {
        private readonly ContentDbContext _DbContext;
        private readonly IPublishingId _PublishingId;
        private readonly ILogger _Logger;
        private readonly IHttpResponseHeaderConfig _HttpResponseHeaderConfig;

        public HttpGetCdnContentCommand(ContentDbContext dbContext, IPublishingId publishingId, ILogger<HttpGetCdnContentCommand> logger, IHttpResponseHeaderConfig httpResponseHeaderConfig)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _PublishingId = publishingId ?? throw new ArgumentNullException(nameof(publishingId));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _HttpResponseHeaderConfig = httpResponseHeaderConfig ?? throw new ArgumentNullException(nameof(httpResponseHeaderConfig));
        }

        public async Task Execute(HttpContext httpContext, string id)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            //This looked like a bug?
            if (!httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                _Logger.LogError($"Required request header missing - if-none-match.");
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400; //TODO!
            }

            if (!_PublishingId.Validate(id))
            {
                _Logger.LogError($"Invalid content id - {id}.");
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            var content = await _DbContext.SafeGetContent(id);
            
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

            //var accepts = httpContext.Request.Headers["accept"].ToHashSet();
            //var signedResponse = content.SignedContentTypeName != null && accepts.Contains(content.SignedContentTypeName);

            //if (!signedResponse && !accepts.Contains(content.ContentTypeName))
            //{
            //    _Logger.LogWarning($"Cannot give acceptable response, responding with 406 - {id}.");
            //    httpContext.Response.StatusCode = 406;
            //    httpContext.Response.ContentLength = 0;
            //    return;
            //}

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", content.ContentTypeName);
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = content.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            await httpContext.Response.Body.WriteAsync(content.Content);
        }

        public async Task Execute(HttpContext httpContext, string genericContentName, string id)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (!ContentTypes.IsValid(genericContentName))
            {
                _Logger.LogError($"Invalid generic content type - {id}.");
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            if (!_PublishingId.Validate(id))
            {
                _Logger.LogError($"Invalid content id - {id}.");
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            //This looked like a bug?
            if (!httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                _Logger.LogError($"Required request header missing - if-none-match.");
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400; //TODO!
            }

            var content = await _DbContext.SafeGetContent(genericContentName, id);
            
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

            //var accepts = httpContext.Request.Headers["accept"].ToHashSet();
            //var signedResponse = content.SignedContentTypeName != null && accepts.Contains(content.SignedContentTypeName);

            //if (!signedResponse && !accepts.Contains(content.ContentTypeName))
            //{
            //    _Logger.LogWarning($"Cannot give acceptable response, responding with 406 - {id}.");
            //    httpContext.Response.StatusCode = 406;
            //    httpContext.Response.ContentLength = 0;
            //    return;
            //}

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", content.ContentTypeName);
            httpContext.Response.Headers.Add("cache-control", _HttpResponseHeaderConfig.ImmutableContentCacheControl);
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = content.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            await httpContext.Response.Body.WriteAsync(content.Content);
        }
    }
}