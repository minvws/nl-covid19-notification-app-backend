// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
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
        private readonly IThumbprintConfig _config;
        private readonly IAuthenticationCertificateProvider _certificateProvider;
        private readonly IksUploaderLoggingExtensions _logger;

        public IksUploadService(HttpClient httpClient, IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, IksUploaderLoggingExtensions logger, IThumbprintConfig config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _certificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<HttpPostIksResult> ExecuteAsync(IksSendCommandArgs args)
        {
            var uri = new Uri($"{_efgsConfig.BaseUrl}/diagnosiskeys/upload");

            // Configure authentication certificate
            using var clientCert = _certificateProvider.GetCertificate(_config.Thumbprint, _config.RootTrusted);
            using var clientHandler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };

            // Provide the authentication certificate manually
            clientHandler.ClientCertificates.Clear();
            clientHandler.ClientCertificates.Add(clientCert);

            _logger.WriteRequestContent(args.Content);           

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
                _logger.WriteEfgsError(e);

                if (e.InnerException != null)
                {
                    _logger.WriteEfgsInnerException(e.InnerException);
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

            _logger.WriteRequest(request);

            return request;
        }
    }
}
