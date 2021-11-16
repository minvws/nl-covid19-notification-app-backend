// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Tests.IksOutbound
{
    public class FakeHttpClient : HttpClient
    {
        public new async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            await Task.Delay(0);

            var httpResponseMessage = new HttpResponseMessage() { StatusCode = HttpStatusCode.OK };

            return httpResponseMessage;
        }
    }
}
