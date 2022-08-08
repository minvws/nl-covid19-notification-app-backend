// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
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
        private readonly ICertificateProvider _certificateProvider;

        public HsmSignerService(
            HttpClient httpClient,
            IHsmSignerConfig config,
            ICertificateProvider certificateProvider
            )
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _certificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));

            _httpClient.BaseAddress = new Uri("");

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<byte[]> GetNlSignatureAsync(byte[] content)
        {
            using var sha256Hasher = SHA256.Create();
            var contentHash = sha256Hasher.ComputeHash(content);
            var contentHashBased64 = Convert.ToBase64String(contentHash);

            // Get EV RSA certificate from certificate store
            var certificate = _certificateProvider.GetCertificate(
                "",
                false);
            var certificateChain = GetCertificateChain();

            var builder = new StringBuilder();

            // Add EV RSA certificate
            builder.AppendLine(new string(PemEncoding.Write("CERTIFICATE", certificate.RawData)));

            // Add EV certificate chain
            foreach (var cert in certificateChain)
            {
                builder.AppendLine(new string(PemEncoding.Write("CERTIFICATE", cert.RawData)));
            }

            var pem = builder.ToString();
            var pemFromConfig = _config.NlPem;

            var pemBytes = Encoding.UTF8.GetBytes(pem);
            var pemBytesBased64 = Convert.ToBase64String(pemBytes);

            var requestModel = new HsmSignerRequestModel()
            {
                Algorithm = "sha256",
                Cert = pemBytesBased64,
                Hash = contentHashBased64,
                TimeStamp = true
            };

            _httpClient.DefaultRequestHeaders.Add("Authorization", _config.NlJwt);
            var response = await _httpClient.PostAsJsonAsync("/cms", requestModel);

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]> GetGaenSignatureAsync(byte[] content)
        {
            using var sha256Hasher = SHA256.Create();
            var contentHash = sha256Hasher.ComputeHash(content);
            var contentHashBased64 = Convert.ToBase64String(contentHash);

            // Get GAEN ECDSA certificate from certificate store
            var certificate = _certificateProvider.GetCertificate(
                "",
                false);

            // Add GAEN ECDSA certificate
            var pem = new string(PemEncoding.Write("CERTIFICATE", certificate.RawData));
            var pemFromConfig = _config.GaenPem;

            var pemBytes = Encoding.UTF8.GetBytes(pem);
            var pemBytesBased64 = Convert.ToBase64String(pemBytes);

            var requestModel = new HsmSignerRequestModel()
            {
                Algorithm = "sha256",
                Cert = pemBytesBased64,
                Hash = contentHashBased64,
                TimeStamp = true
            };

            _httpClient.DefaultRequestHeaders.Add("Authorization", _config.GaenJwt);
            var response = await _httpClient.PostAsJsonAsync("/signature", requestModel);

            return await response.Content.ReadAsByteArrayAsync();
        }

        private X509Certificate2[] GetCertificateChain()
        {
            var certList = new List<X509Certificate2>();

            using var s = ResourcesHook.GetManifestResourceStream(
                "StaatDerNLChain-EV-Expires-2022-12-05.p7b");

            if (s == null)
            {
                throw new InvalidOperationException($"Certificate chain not found in resource.");
            }

            var bytes = new byte[s.Length];
            s.Read(bytes, 0, bytes.Length);

            var result = new X509Certificate2Collection();
            result.Import(bytes);
            foreach (var c in result)
            {
                if (c.IssuerName.Name != c.SubjectName.Name)
                {
                    certList.Add(c);
                }
            }

            return certList.ToArray();
        }
    }
}
