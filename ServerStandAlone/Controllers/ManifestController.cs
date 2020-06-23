// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using Swashbuckle.AspNetCore.Annotations;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.ServerStandAlone.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ManifestController : ControllerBase
    {
        /// <response code="200">Manifest retrieved successfully.</response>
        [HttpGet]
        [Route(EndPointNames.CdnApi.Manifest)]
        [SwaggerOperation(
            Summary = "Get the manifest containing identifiers for the content on the CDN.",
            Description = "The manifest file should be periodically retrieved and provides the app with details about content that can be downloaded/updated." +
                          "\nThe frequency of retrieving the manifest should be defined based on the value manifestFrequency in the AppConfig, with a certain randomness to spread the load on the CDN over time." +
                          "\nTODO: Define randomness formula for retrieving the manifest request.",
            OperationId = "getManifest", //TODO Is this correct?
            Tags = new[] { "CDN" }
        )] //Adds nothing to UI
        [ProducesResponseType(typeof(ManifestContent), 200)]
        //[ProducesResponseType(typeof(byte[]), 200)] //TODO No added value in Swagger UI
        [ProducesResponseType(500)]
        [Produces(MediaTypeNames.Application.Zip, MediaTypeNames.Application.Json)]
        public async Task GetLatestConfig([FromServices]HttpGetManifestSasCommand command)
        {
            await command.Execute(HttpContext);
        }
    }

    public class HttpGetManifestSasCommand
    {
        private readonly DynamicManifestReader _DynamicManifestReader;

        public HttpGetManifestSasCommand(DynamicManifestReader dynamicManifestReader)
        {
            _DynamicManifestReader = dynamicManifestReader;
        }

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
                await httpContext.Response.BodyWriter.WriteAsync(e.SignedContent);

                return;
            }

            httpContext.Response.StatusCode = 415;
        }
    }
}
