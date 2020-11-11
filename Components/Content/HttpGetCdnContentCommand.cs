﻿// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.GetCdnContent;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using System;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
	/// <summary>
	/// Includes mitigations for CDN cache miss/stale item edge cases.
	/// </summary>
	public class HttpGetCdnContentCommand
    {
        private readonly ContentDbContext _DbContext;
        private readonly IPublishingIdService _PublishingIdService;
        private readonly GetCdnContentLoggingExtensions _Logger;
        private readonly IUtcDateTimeProvider _DateTimeProvider;

        public HttpGetCdnContentCommand(ContentDbContext dbContext, IPublishingIdService publishingIdService, GetCdnContentLoggingExtensions logger, IUtcDateTimeProvider dateTimeProvider)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _PublishingIdService = publishingIdService ?? throw new ArgumentNullException(nameof(publishingIdService));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        /// <summary>
        /// Immutable content
        /// </summary>
        public async Task<ContentEntity?> Execute(HttpContext httpContext, string type, string id)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            if (!ContentTypes.IsValid(type))
            {
                _Logger.WriteInvalidType(id);
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            if (!_PublishingIdService.Validate(id))
            {
                _Logger.WriteInvalidId(id);
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            if (!httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                _Logger.WriteHeaderMissing();
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400;
            }

            var content = await _DbContext.SafeGetContent(type, id, _DateTimeProvider.Snapshot);
            
            if (content == null)
            {
                _Logger.WriteNotFound(id);
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return null;
            }

            if (etagValue == content.PublishingId)
            {
                _Logger.WriteEtagFound(id);
                httpContext.Response.StatusCode = 304;
                httpContext.Response.ContentLength = 0;
                return null;
            }

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.Release.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", content.ContentTypeName);
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentLength = content.Content?.Length ?? throw new InvalidOperationException("SignedContent empty.");
            return content;
        }
    }
}