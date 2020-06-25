// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using System.IO.Pipelines;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpGetManifestSasCommand
    {
        private readonly DynamicManifestReader _DynamicManifestReader;

        public HttpGetManifestSasCommand(DynamicManifestReader dynamicManifestReader)
        {
            _DynamicManifestReader = dynamicManifestReader;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task Execute(HttpContext httpContext)
        {
            var e = await _DynamicManifestReader.Execute();

            if (e == null) throw new InvalidOperationException();

            if (!httpContext.Request.Headers.TryGetValue("accept", out var contentList))
            {
                httpContext.Response.StatusCode = 415;

                return;
            }

            if (contentList.Contains(MediaTypeNames.Application.Json))
            {
                httpContext.Response.ContentLength = e.Content.Length;
                httpContext.Response.StatusCode = 200;
                httpContext.Response.Headers.Add("content-type", e.ContentTypeName);
                await httpContext.Response.BodyWriter.WriteAsync(e.Content);

                return;
            }

            if (contentList.Contains(MediaTypeNames.Application.Zip))
            {
                httpContext.Response.ContentLength = e.SignedContent.Length;
                httpContext.Response.StatusCode = 200;
                httpContext.Response.Headers.Add("content-type", e.SignedContentTypeName);
                httpContext.Response.Headers.Add("etag", e.PublishingId);
                await httpContext.Response.BodyWriter.WriteAsync(e.SignedContent);

                return;
            }

            httpContext.Response.StatusCode = 415;
        }
    }
}