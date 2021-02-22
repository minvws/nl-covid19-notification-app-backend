// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    /// <summary>
    /// The generic API client using a Polly circuit breaker
    /// </summary>
    public class RestApiClient : IRestApiClient
    {
        private readonly HttpClient _HttpClient;
        private readonly ILogger<RestApiClient> _Logger;

        public RestApiClient(HttpClient httpClient, ILogger<RestApiClient> logger)
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
            try
            {
                _Logger.LogInformation($"BaseUrl: {_HttpClient.BaseAddress}");
                _Logger.LogInformation($"RequestUri: {requestUri}");

                var response = await _HttpClient.GetAsync(requestUri, token);

                if (response.IsSuccessStatusCode)
                {
                    return new OkObjectResult(response);
                }
                else
                {
                    return new BadRequestResult();
                }
            }
            catch (Exception e)
            {
                _Logger.LogError($"Error in GET from: {_HttpClient.BaseAddress}/{requestUri}.", e.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Post a model to a given endpoint.
        /// </summary>
        /// <typeparam name="T">The model type to po posted</typeparam>
        /// <param name="model">The actual model</param>
        /// <param name="requestUri">The uri to post to</param>
        /// <param name="token">The generated cancellation token</param>
        /// <returns></returns>
        public async Task<IActionResult> PostAsync<T>(T model, string requestUri, CancellationToken token) where T : class
        {
            try
            {
                var message = JsonConvert.SerializeObject(model, Formatting.None);

                HttpContent httpContent = new StringContent(message, Encoding.UTF8, "application/json");
                var response = await _HttpClient.PostAsync(requestUri, httpContent, token);

                if (response.IsSuccessStatusCode)
                {
                    return new OkObjectResult(response);
                }
                else
                {
                    return new BadRequestResult();
                }
            }
            catch (Exception e)
            {
                _Logger.LogError($"Error in POST to: {_HttpClient.BaseAddress}/{requestUri}.", e.ToString());
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
