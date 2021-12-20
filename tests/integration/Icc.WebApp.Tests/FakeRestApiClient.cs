// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace Icc.WebApp.Tests
{
    public class FakeRestApiClient : IRestApiClient
    {
        private readonly HttpClient _httpClient;

        public FakeRestApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Uri BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken token)
        {
            return new HttpResponseMessage();
        }

        public async Task<HttpResponseMessage> PostAsync<T>(T model, string requestUri, CancellationToken token) where T : class
        {
            var args = (PublishTekArgs)Convert.ChangeType(model, typeof(PublishTekArgs));
            if (args.GGDKey == "111111")
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new PublishTekResponse { Valid = true })) };
        }

        public async Task<HttpResponseMessage> PutAsync<T>(T model, string requestUri, CancellationToken token, string refererName = "") where T : class
        {
            var args = (PublishTekArgs)Convert.ChangeType(model, typeof(PublishTekArgs));
            if (args.GGDKey == "111111")
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new PublishTekResponse { Valid = true })) };
        }
    }
}
