using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.Authorisation;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace App.IccPortal.Tests
{
    public class FakeRestApiClient : IRestApiClient
    {
        private readonly HttpClient _HttpClient;
        private readonly ILogger<FakeRestApiClient> _Logger;

        public FakeRestApiClient(HttpClient httpClient, ILogger<FakeRestApiClient> logger)
        {
            _HttpClient = httpClient;
            _Logger = logger;
        }

        public Uri BaseAddress
        {
            get => _HttpClient.BaseAddress;
            set => _HttpClient.BaseAddress = value;
        }

        public async Task<IActionResult> GetAsync(string requestUri, CancellationToken token)
        {
            return new OkResult();
        }

        public async Task<IActionResult> PostAsync<T>(T model, string requestUri, CancellationToken token) where T : class
        {
            var args = (PublishTekArgs)Convert.ChangeType(model, typeof(PublishTekArgs));
            if (args.GGDKey == "111111")
            {
                return new BadRequestResult();
            }
            return new OkObjectResult(new PublishTekResponse { Valid = true });
        }

        public async Task<IActionResult> PutAsync<T>(T model, string requestUri, CancellationToken token) where T : class
        {
            var args = (PublishTekArgs)Convert.ChangeType(model, typeof(PublishTekArgs));
            if (args.GGDKey == "111111")
            {
                return new BadRequestResult();
            }
            return new OkObjectResult(new PublishTekResponse{ Valid = true });
        }
    }
}
