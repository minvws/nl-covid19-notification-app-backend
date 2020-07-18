// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Services
{
    public class AppBackendService
    {
        private readonly string _BaseUrl;
        private readonly string _Prefix;
        private readonly HttpClient _HttpClient = new HttpClient();
        private readonly IJsonSerializer _JsonSerializer;

        public AppBackendService(IConfiguration configuration, IJsonSerializer jsonSerializer)
        {
            _JsonSerializer = jsonSerializer;
            _BaseUrl = configuration.GetSection("AppBackendConfig:BaseUri").Value;
            _Prefix = configuration.GetSection("AppBackendConfig:Prefix").Value;
        }

        private string GetAppBackendUrl(string endpoint)
        {
            return _BaseUrl + (endpoint.StartsWith("/") ? endpoint : "/" + _Prefix + "/" + endpoint);
        }

        private async Task<string> BackendPostRequest(string endpoint, object payload)
        {
            var jsonPayload = _JsonSerializer.Serialize(payload);
            var url = GetAppBackendUrl(endpoint);
            try
            {
                var response = await _HttpClient.PostAsync(url,
                    new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<bool> LabConfirmationIdIsValid(ConfirmLabConfirmationIdModel confirmLabConfirmationIdModel)
        {
            var backendResponse =
                await BackendPostRequest("/v1/labConfirm",
                    new AuthorisationArgs(confirmLabConfirmationIdModel));

            if (backendResponse == null) return false;
            var backendResponseJson = _JsonSerializer.Deserialize<Dictionary<string, bool>>(backendResponse);
            return backendResponseJson != null && backendResponseJson["valid"];
        }
    }
}