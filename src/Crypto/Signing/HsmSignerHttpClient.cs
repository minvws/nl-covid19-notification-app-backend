// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Signing
{
    /// <summary>
    /// Typed HttpClient to request a signature from the HsmSignerService API
    /// </summary>
    public class HsmSignerHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _jwt = "";

        public HsmSignerHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.BaseAddress = new Uri("https://");

            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
            _httpClient.DefaultRequestHeaders.Add("Authorization", _jwt);
        }

        public async Task<byte[]> GetSignatureAsync(byte[] content, X509Certificate2 certificate, X509Certificate2[] certificateChain)
        {
            using var sha256Hasher = SHA256.Create();
            var contentHash = sha256Hasher.ComputeHash(content);

            var builder = new StringBuilder();

            //TODO: check what certificate info is needed, and how the API wants to receive it

            builder.AppendLine("-----BEGIN CERTIFICATE-----");
            Convert.ToBase64String(certificate.RawData, Base64FormattingOptions.InsertLineBreaks);
            builder.AppendLine("-----END CERTIFICATE-----");

            foreach (var cert in certificateChain)
            {
                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                Convert.ToBase64String(cert.RawData, Base64FormattingOptions.InsertLineBreaks);
                builder.AppendLine("-----END CERTIFICATE-----");
            }

            var pemCert = builder.ToString();

            var response = await _httpClient.PostAsJsonAsync("", new
                {
                    algorithm = "sha256",
                    cert = pemCert,
                    hash = contentHash,
                    timestamp = false
                });

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}