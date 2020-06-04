// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
{
    public class HttpGetLatestManifestCommand
    {
        private readonly HttpGetCdnContentCommand _Handler;
        private readonly GetLatestManifestCommand _Reader;

        public HttpGetLatestManifestCommand(GetLatestManifestCommand reader, HttpGetCdnContentCommand handler)
        {
            _Reader = reader;
            _Handler = handler;
        }

        public void Execute(HttpContext httpContext)
        {
            var e = _Reader.Execute();
            var r = new BinaryContentResponse
            {
                PublishingId = e.PublishingId,
                LastModified = e.Release,
                ContentTypeName = e.ContentTypeName,
                Content = e.Content
            };
            _Handler.Execute(httpContext, r);
        }
    }
}