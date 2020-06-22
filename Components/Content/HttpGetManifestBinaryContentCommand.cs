using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    /// <summary>
    /// Used in the Cdn Data API in the Datacentre/Business Zone
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HttpGetManifestBinaryContentCommand
    {
        private readonly DynamicManifestReader _DynamicManifestReader;

        public HttpGetManifestBinaryContentCommand(DynamicManifestReader dynamicManifestReader)
        {
            _DynamicManifestReader = dynamicManifestReader;
        }

        public async Task<IActionResult> Execute(HttpContext httpContext)
        {
            var e = await _DynamicManifestReader.Execute();

            var r = new BinaryContentResponse
            {
                LastModified = e.Release,
                PublishingId = e.PublishingId,
                ContentTypeName = e.ContentTypeName,
                Content = e.Content,
                SignedContentTypeName = e.SignedContentTypeName,
                SignedContent = e.SignedContent
            };

            return new OkObjectResult(r);
        }
    }
}