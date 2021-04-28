// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet
{
    /// <summary>
    /// The generic API client
    /// </summary>
    public class RestApiClient : IRestApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestApiClient> _logger;

        public RestApiClient(HttpClient httpClient, ILogger<RestApiClient> logger)
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
            try
            {
                _logger.LogInformation($"BaseUrl: {_httpClient.BaseAddress}");
                _logger.LogInformation($"RequestUri: {requestUri}");

                var response = await _httpClient.GetAsync(requestUri, token);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error in GET from: {_httpClient.BaseAddress}/{requestUri}.", e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
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
        public async Task<HttpResponseMessage> PostAsync<T>(T model, string requestUri, CancellationToken token) where T : class
        {
            try
            {
                var message = JsonConvert.SerializeObject(model, Formatting.None);

                HttpContent httpContent = new StringContent(message, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(requestUri, httpContent, token);
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error in POST to: {_httpClient.BaseAddress}/{requestUri}.", e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
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
        public async Task<HttpResponseMessage> PutAsync<T>(T model, string requestUri, CancellationToken token) where T : class
        {
            try
            {
                var message = JsonConvert.SerializeObject(model, Formatting.None);

                HttpContent httpContent = new StringContent(message, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(requestUri, httpContent, token);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error in POST to: {_httpClient.BaseAddress}/{requestUri}.", e.ToString());
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
