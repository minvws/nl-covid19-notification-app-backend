using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace App.IccPortal.Tests
{
    public class FakeRestApiClient : IRestApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FakeRestApiClient> _logger;

        public FakeRestApiClient(HttpClient httpClient, ILogger<FakeRestApiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

        public async Task<HttpResponseMessage> PutAsync<T>(T model, string requestUri, CancellationToken token) where T : class
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
