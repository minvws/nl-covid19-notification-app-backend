// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    /// <summary>
    /// Typed HttpClient to request a signature from the HsmSignerService API
    /// </summary>
    public class HsmSignerService : IHsmSignerService
    {
        private readonly HttpClient _httpClient;
        private readonly IHsmSignerConfig _config;

        public HsmSignerService(
            HttpClient httpClient,
            IHsmSignerConfig config
            )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            _httpClient.BaseAddress = new Uri(_config.BaseAddress);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<byte[]> GetCmsSignatureAsync(byte[] content)
        {
            using var sha256Hasher = SHA256.Create();
            var contentHash = sha256Hasher.ComputeHash(content);
            var contentHashBased64 = Convert.ToBase64String(contentHash);

            var cmsPublicCertificateChain = _config.CmsPublicCertificateChain;

            var cmsCertBytes = Encoding.UTF8.GetBytes(cmsPublicCertificateChain);
            var cmsCertBytesBased64 = Convert.ToBase64String(cmsCertBytes);

            var requestModel = new HsmSignerRequestModel()
            {
                Algorithm = "sha256",
                Cert = cmsCertBytesBased64,
                Hash = contentHashBased64,
                TimeStamp = true
            };

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", _config.CmsJwt);

            var response = _httpClient.PostAsJsonAsync("/cms", requestModel).Result;

            if (!response.IsSuccessStatusCode)
            {
                //TODO: error handling
                return Array.Empty<byte>();
            }

            var responseModel = await response.Content.ReadFromJsonAsync<HsmSignerCmsResponseModel>();
            if (responseModel == null)
            {
                //TODO: error handling
                return Array.Empty<byte>();
            }

            var cms = responseModel.Cms;

            var cmsBytes = Convert.FromBase64String(cms);
            var cmsString = Encoding.UTF8.GetString(cmsBytes);

            var result = StripCmsString(cmsString);
            return Convert.FromBase64String(result);
        }

        public async Task<byte[]> GetGaenSignatureAsync(byte[] content)
        {
            using var sha256Hasher = SHA256.Create();
            var contentHash = sha256Hasher.ComputeHash(content);
            var contentHashBased64 = Convert.ToBase64String(contentHash);

            var gaenPublicCertificate = _config.GaenPublicCertificate;

            var gaenCertBytes = Encoding.UTF8.GetBytes(gaenPublicCertificate);
            var gaenCertBytesBased64 = Convert.ToBase64String(gaenCertBytes);

            var requestModel = new HsmSignerRequestModel()
            {
                Algorithm = "sha256",
                Cert = gaenCertBytesBased64,
                Hash = contentHashBased64,
                TimeStamp = true
            };

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", _config.GaenJwt);

            var response = await _httpClient.PostAsJsonAsync("/signature", requestModel);

            if (!response.IsSuccessStatusCode)
            {
                //TODO: error handling
                return Array.Empty<byte>();
            }

            var responseModel = await response.Content.ReadFromJsonAsync<HsmSignerSignatureResponseModel>();
            if (responseModel == null)
            {
                //TODO: error handling
                return Array.Empty<byte>();
            }

            var signature = responseModel.Signature;

            return Convert.FromBase64String(signature);
        }

        private string StripCmsString(string cmsString)
        {
            cmsString = cmsString.Replace("\r", string.Empty);
            cmsString = cmsString.Replace("\n", string.Empty);
            cmsString = cmsString.Substring("-----BEGIN CMS-----".Length);
            cmsString = cmsString.Substring(0, cmsString.Length - "-----END CMS-----".Length);

            return cmsString;
        }
    }
}
