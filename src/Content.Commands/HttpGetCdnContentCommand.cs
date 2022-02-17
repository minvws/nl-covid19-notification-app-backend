// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    /// <summary>
    /// Includes mitigations for CDN cache miss/stale item edge cases.
    /// </summary>
    public class HttpGetCdnContentCommand
    {
        private readonly ContentDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IUtcDateTimeProvider _dateTimeProvider;

        public HttpGetCdnContentCommand(
            ContentDbContext dbContext,
            ILogger<HttpGetCdnContentCommand> logger,
            IUtcDateTimeProvider dateTimeProvider)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        /// <summary>
        /// Immutable content
        /// </summary>
        public async Task<ContentEntity> ExecuteAsync(HttpContext httpContext, ContentTypes type, string id)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // Validate both guid and hash-based id while we're transitioning from hash-based id to guid
            if (!Guid.TryParse(id, out _) && !ValidateHash(id))
            {
                _logger.LogError("Invalid content id - {ContentId}.", id);
                httpContext.Response.StatusCode = 400;
                httpContext.Response.ContentLength = 0;
            }

            if (!httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                _logger.LogDebug("Required request header missing - if-none-match.");
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400;
            }

            var content = await _dbContext.SafeGetContentAsync(type, id, _dateTimeProvider.Snapshot);

            if (content == null)
            {
                _logger.LogError("Content not found - {ContentId}.", id);
                httpContext.Response.StatusCode = 404;
                httpContext.Response.ContentLength = 0;
                return null;
            }

            if (etagValue == content.PublishingId)
            {
                _logger.LogWarning("Matching etag found, responding with 304 - {ContentId}.", id);
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

        private static bool ValidateHash(string id)
        {
            const int Sha256Length = 32;
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            if (id.Length != Sha256Length * 2)
            {
                return false;
            }

            for (var i = 0; i < id.Length; i += 2)
            {
                if (!int.TryParse(id.Substring(i, 2), NumberStyles.HexNumber, NumberFormatInfo.InvariantInfo, out _))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
