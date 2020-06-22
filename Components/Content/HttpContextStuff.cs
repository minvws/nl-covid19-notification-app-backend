// Copyright  De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content
{
    public static class HttpContextStuff
    {
        public static async Task RespondWith<T>(this HttpContext httpContext, T t)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, t);
            var buffer = stream.ToArray();

            httpContext.Response.StatusCode = 200;
            httpContext.Response.Headers.Add("content-type", MediaTypeNamesAdditional.Application.Protobuf);
            httpContext.Response.ContentLength = buffer.Length;
            await httpContext.Response.Body.WriteAsync(buffer);
        }
    }
}