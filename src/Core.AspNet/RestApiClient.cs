// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
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
                _logger.LogInformation("BaseUrl: {BaseAddress}", _httpClient.BaseAddress);
                _logger.LogInformation("RequestUri: {RequestUri}", requestUri);

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
                _logger.LogError(e, "Error in GET from: {BaseAddress}/{RequestUri}",
                    _httpClient.BaseAddress, requestUri);
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
                _logger.LogError(e, "Error in POST to: {BaseAddress}/{RequestUri}",
                    _httpClient.BaseAddress, requestUri);
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
        /// <param name="refererName">Optional name of caller</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutAsync<T>(T model, string requestUri, CancellationToken token, string refererName = "") where T : class
        {
            try
            {
                var message = JsonConvert.SerializeObject(model, Formatting.None);

                HttpContent httpContent = new StringContent(message, Encoding.UTF8, "application/json");
                httpContent.Headers.Add("RefererName", new List<string> { refererName });
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
                _logger.LogError(e, "Error in POST to: {BaseAddress}/{RequestUri}",
                    _httpClient.BaseAddress, requestUri);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
