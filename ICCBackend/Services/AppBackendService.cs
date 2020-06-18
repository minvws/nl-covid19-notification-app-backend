// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NL.Rijksoverheid.ExposureNotification.IccBackend.Models;

namespace NL.Rijksoverheid.ExposureNotification.IccBackend.Services
{
    public class AppBackendService
    {
        private IConfiguration _Configuration;
        public AppBackendService(IConfiguration _configuration)
        {
            _Configuration = _configuration;
        }

        private HttpClient CreateBackendHttpClient(string endpoint)
        {
            var BaseURL = "https://" + _Configuration.GetSection("AppBackendConfig:Host").Value.ToString() + "/";
            // TODO: Make HTTPClient with urls;
            return new HttpClient();
        }

        private object BackendGetRequest()
        {
            return new {};
        }
        private object BackendPostRequest(object payload)
        {
            return new {};
            
        }
        
        public async Task<bool> LabConfirmationIdIsValid(RedeemIccModel redeemIccModel)
        {
            Console.WriteLine(_Configuration.GetSection("AppBackendConfig:Host"));
            
            // post to labresult with id and date params
            
            var backendResponse = BackendPostRequest(redeemIccModel);
            
            
            
            return true;
        }
    }
}