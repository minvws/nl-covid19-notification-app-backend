// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Authorisation;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Models;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Services
{
    public class AppBackendService
    {
        private readonly string _BaseUrl;
        private readonly HttpClient _HttpClient = new HttpClient();
        private readonly bool _AuthenticationEnabled = true;

        public AppBackendService(IConfiguration configuration)
        {
            _BaseUrl = configuration.GetSection("AppBackendConfig:BaseUri").Value.ToString();
            if (_AuthenticationEnabled)
            {
                // TODO: Move to settings service
                _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(System.Convert.FromBase64String(
                        configuration.GetSection("AppBackendConfig:Authentication:UserName").Value + ":" +
                        configuration.GetSection("AppBackendConfig:Authentication:Password").Value)));
            }
        }

        private string GetAppBackendUrl(string endpoint)
        {
            return _BaseUrl + (endpoint.StartsWith("/") ? endpoint : "/" + endpoint);
        }

        private async Task<string> BackendGetRequest(string endpoint)
        {
            try
            {
                HttpResponseMessage response = await _HttpClient.GetAsync(GetAppBackendUrl(endpoint));
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private async Task<string> BackendPostRequest(string endpoint, object payload)
        {
            string jsonPayload = JsonConvert.SerializeObject(payload);
            try
            {
                HttpResponseMessage response = await _HttpClient.PostAsync(GetAppBackendUrl(endpoint),
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

        public async Task<bool> LabConfirmationIdIsValid(RedeemIccModel redeemIccModel)
        {
            var backendResponse =
                await BackendPostRequest(EndPointNames.CaregiversPortalApi.LabConfirmation,
                    new AuthorisationArgs(redeemIccModel));

            if (backendResponse == null) return false;
            var backendResponseJson = JsonConvert.DeserializeObject<dynamic>(backendResponse);
            return backendResponseJson != null && backendResponseJson.Valid;
        }
    }
}