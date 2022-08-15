// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;
using Polly.CircuitBreaker;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksUploadService
    {
        private readonly HttpClient _httpClient;

        private readonly IEfgsConfig _efgsConfig;
        private readonly IAuthenticationCertificateProvider _efgsCertificateProvider;
        private readonly ICertificateConfig _certificateConfig;
        private readonly ILogger _logger;

        public IksUploadService(
            HttpClient httpClient,
            IEfgsConfig efgsConfig,
            IAuthenticationCertificateProvider certificateProvider,
            ICertificateConfig certificateConfig,
            ILogger<IksUploadService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _efgsCertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _certificateConfig = certificateConfig ?? throw new ArgumentNullException(nameof(certificateConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpPostIksResult> ExecuteAsync(IksSendCommandArgs args)
        {
            var uri = new Uri($"{_efgsConfig.BaseUrl}/diagnosiskeys/upload");

            // Configure authentication certificate
            using var clientCert = _efgsCertificateProvider.GetCertificate();

            _logger.LogDebug("EFGS request content: {Content}", Convert.ToBase64String(args.Content));

            try
            {
                var response = await _httpClient.SendAsync(BuildRequest(args, uri, clientCert));

                return new HttpPostIksResult
                {
                    HttpResponseCode = response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
            }
            catch (BrokenCircuitException e)
            {
                _logger.LogError(e, "Error calling EFGS, see exception for details.");

                if (e.InnerException != null)
                {
                    _logger.LogError(e.InnerException,
                        "Error calling EFGS, see inner exception for further details.");
                }

                return new HttpPostIksResult
                {
                    Exception = true
                };
            }
        }

        private HttpRequestMessage BuildRequest(IksSendCommandArgs args, Uri uri, X509Certificate2 clientCert)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new ByteArrayContent(args.Content)
            };

            request.Content.Headers.Add("Content-Type", "application/protobuf; version=1.0");
            request.Headers.Add("BatchTag", args.BatchTag);
            request.Headers.Add("batchSignature", Convert.ToBase64String(args.Signature));
            request.Headers.Add("Accept", "application/json;version=1.0");

            if (_efgsConfig.SendClientAuthenticationHeaders)
            {
                request.Headers.Add("X-SSL-Client-SHA256", clientCert.ComputeSha256Hash());
                request.Headers.Add("X-SSL-Client-DN", clientCert.Subject.Replace(" ", string.Empty));
            }

            _logger.LogInformation("EFGS request: {Request}", request);

            return request;
        }
    }
}
