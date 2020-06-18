// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Net.Http.Headers;

//namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets
//{
//    /// <summary>
//    /// TODO depending on accept-headers grab the right content and return
//    /// </summary>
//    public class HttpGetExposureKeySetCommand
//    {
//        private readonly ExposureKeySetSafeReadCommand _Reader;

//        public HttpGetExposureKeySetCommand(ExposureKeySetSafeReadCommand reader)
//        {
//            _Reader = reader;
//        }

//        public void Execute(string id, HttpContext httpContext)
//        {
//            var result = _Reader.Execute(id);

//            if (result == null)
//            {
//                httpContext.Response.StatusCode = 404;
//                return;
//            }
            
            
            
//            return new FileContentResult(result.Content, MediaTypeHeaderValue.Parse("application/zip"));
//        }
//    }
//}