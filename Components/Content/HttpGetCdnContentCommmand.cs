// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public class HttpGetCdnContentCommmand
    {

        public void Execute(HttpContext httpContext, BinaryContentResponse content)
        {
            if (httpContext.Request.Headers.TryGetValue("if-none-match", out var etagValue))
            {
                httpContext.Response.ContentLength = 0;
                httpContext.Response.StatusCode = 400;
            }

            //Read
            
            if (content == null)
            {
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

            httpContext.Response.Headers.Add("etag", content.PublishingId);
            httpContext.Response.Headers.Add("last-modified", content.LastModified.ToUniversalTime().ToString("r"));
            httpContext.Response.Headers.Add("content-type", content.ContentTypeName);
            httpContext.Response.Body.Write(content.Content);
        }
    }
}