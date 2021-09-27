// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace Scenario.Tests
{
    public class AppClient
    {
        private readonly IJsonSerializer _jsonSerializer;
        private HttpClient _client;

        public AppClient()
        {
            _jsonSerializer = new StandardJsonSerializer();
        }

        protected T ParseContent<T>(string message)
        {
            return _jsonSerializer.Deserialize<T>(message);
        }

        public async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            _client = new HttpClient { BaseAddress = uri };
            var responseMessage = await _client.GetAsync("");

            return responseMessage;
        }

        public async Task<HttpResponseMessage> PostAsync(Uri uri, string version, string endpoint, HttpContent content)
        {
            _client = new HttpClient { BaseAddress = uri };
            return await _client.PostAsync($"{version}/{endpoint}", content);
        }

        public async Task<(HttpResponseMessage, T)> PostAsync<T>(Uri uri, string version, string endpoint, HttpContent content) where T : new()
        {
            _client = new HttpClient { BaseAddress = uri };
            var responseMessage = await _client.PostAsync($"{version}/{endpoint}", content);
            return (responseMessage, ParseContent<T>(await responseMessage.Content.ReadAsStringAsync()));
        }

        public async Task<HttpResponseMessage> PutAsync(Uri uri, string endpoint, ByteArrayContent content)
        {
            _client = new HttpClient { BaseAddress = uri };
            return await _client.PutAsync($"{endpoint}", content);
        }
    }
}
