// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _Logger; //Actually not used.

        public HttpGetManifestBinaryContentCommand(DynamicManifestReader dynamicManifestReader, ILogger<HttpGetManifestBinaryContentCommand> logger)
        {
            _DynamicManifestReader = dynamicManifestReader ?? throw new ArgumentNullException(nameof(dynamicManifestReader));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Execute()
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
            _Logger.LogInformation("Return new manifest.");
            return new OkObjectResult(r);
        }
    }
}