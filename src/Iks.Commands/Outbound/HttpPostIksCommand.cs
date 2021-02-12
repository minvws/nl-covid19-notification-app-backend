// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net.Http;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class HttpPostIksCommand
    {
        private readonly IEfgsConfig _EfgsConfig;
        private readonly IAuthenticationCertificateProvider _CertificateProvider;
        private readonly IksUploaderLoggingExtensions _Logger;

        public HttpPostIksCommand(IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, IksUploaderLoggingExtensions logger)
        {
            _EfgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpPostIksResult> ExecuteAsync(IksSendCommandArgs args)
        {
            var uri = new Uri($"{_EfgsConfig.BaseUrl}/diagnosiskeys/upload");

            // Configure authentication certificate
            using var clientCert = _CertificateProvider.GetCertificate();
            using var clientHandler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual
            };

            // Provide the authentication certificate manually
            clientHandler.ClientCertificates.Clear();
            clientHandler.ClientCertificates.Add(clientCert);

            // Build request
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Content = new ByteArrayContent(args.Content);
            request.Content.Headers.Add("Content-Type", "application/protobuf; version=1.0");
            request.Headers.Add("BatchTag", args.BatchTag);
            request.Headers.Add("batchSignature", Convert.ToBase64String(args.Signature));
            request.Headers.Add("Accept", "application/json;version=1.0");
            
            if (_EfgsConfig.SendClientAuthenticationHeaders)
            {
                request.Headers.Add("X-SSL-Client-SHA256", clientCert.ComputeSha256Hash());
                request.Headers.Add("X-SSL-Client-DN", clientCert.Subject.Replace(" ", string.Empty));
            }

            _Logger.WriteRequest(request);
            _Logger.WriteRequestContent(args.Content);

            using var client = new HttpClient(clientHandler);
            client.Timeout = TimeSpan.FromSeconds(5); //TODO config

            try
            {
                var response = await client.SendAsync(request);

                return new HttpPostIksResult
                {
                    HttpResponseCode = response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                    //TODO How is this used?
                    //TODO BatchTag = response.Headers.GetSingleValueOrDefault("batchTag")
                };
            }
            catch (Exception e)
            {
                _Logger.WriteEfgsError(e);

                if (e.InnerException != null)
                {
                    _Logger.WriteEfgsInnerException(e.InnerException);
                }

                return new HttpPostIksResult
                {
                    Exception = true
                };
            }
        }
    }
}