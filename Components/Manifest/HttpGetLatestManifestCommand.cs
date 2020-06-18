// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using System;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;
//using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest
//{
//    [Obsolete("Reads from DB.")]
//    public class HttpGetLatestManifestCommand
//    {
//        private readonly HttpGetCdnContentCommand _Handler;
//        private readonly GetLatestManifestCommand _Reader;

//        public HttpGetLatestManifestCommand(GetLatestManifestCommand reader, HttpGetCdnContentCommand handler)
//        {
//            _Reader = reader;
//            _Handler = handler;
//        }

//        public async Task Execute(HttpContext httpContext)
//        {
//            var e = _Reader.Execute();
//            var r = new BinaryContentResponse
//            {
//                PublishingId = e.PublishingId,
//                LastModified = e.Release,
//                ContentTypeName = e.ContentTypeName,
//                Content = e.Content
//            };
//            await _Handler.Execute(httpContext, r);
//        }
//    }
//}