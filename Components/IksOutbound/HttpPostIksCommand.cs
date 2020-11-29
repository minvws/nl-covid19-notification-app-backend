// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksOutbound
{
    public class HttpPostIksCommand
    {
        private readonly IEfgsConfig _EfgsConfig;
        private readonly IAuthenticationCertificateProvider _CertificateProvider;
        private readonly ILogger<HttpPostIksCommand> _Logger;

        public HttpPostIksCommand(IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, ILogger<HttpPostIksCommand> logger)
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
            using var clientHandler = new HttpClientHandler {
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

            _Logger.LogInformation("EFGS request: {request}", request);
            _Logger.LogInformation("EFGS request content: {content}", Convert.ToBase64String(args.Content));

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
                _Logger.LogError("Error calling EFGS, see exception for details");
                _Logger.LogError(e.Message);
                _Logger.LogError(e.StackTrace);

                if (e.InnerException != null)
                {
                    _Logger.LogError("Inner exception:");
                    _Logger.LogError(e.InnerException.Message);
                    _Logger.LogError(e.InnerException.StackTrace);
                }

                return new HttpPostIksResult
                {
                    Exception = true
                };
            }
        }
    }
}