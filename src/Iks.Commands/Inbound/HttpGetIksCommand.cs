// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.AspNet;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Crypto.Certificates;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class HttpGetIksCommand : IIHttpGetIksCommand {

        const string ApplicationProtobuf = "application/protobuf; version=1.0";

        private readonly IEfgsConfig _EfgsConfig;
        private readonly IAuthenticationCertificateProvider _CertificateProvider;
        private readonly IksDownloaderLoggingExtensions _Logger;

        public HttpGetIksCommand(IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, IksDownloaderLoggingExtensions logger)
        {
            _EfgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpGetIksSuccessResult?> ExecuteAsync(string batchTag, DateTime date)
        {
            _Logger.WriteProcessingData(date, batchTag);

            var uri = new Uri($"{_EfgsConfig.BaseUrl}/diagnosiskeys/download/{date:yyyy-MM-dd}");

            try
            {
                // Configure authentication certificate
                using var clientCert = _CertificateProvider.GetCertificate();
                using var clientHandler = new HttpClientHandler {
                    ClientCertificateOptions = ClientCertificateOption.Manual
                };

                // Provide the authentication certificate manually
                clientHandler.ClientCertificates.Clear();
                clientHandler.ClientCertificates.Add(clientCert);

                // Build the request
                var request = new HttpRequestMessage {RequestUri = uri};
                request.Headers.Add("Accept", ApplicationProtobuf);
                if(!string.IsNullOrWhiteSpace(batchTag)) request.Headers.Add("batchTag", batchTag);
                if (_EfgsConfig.SendClientAuthenticationHeaders)
                {
                    request.Headers.Add("X-SSL-Client-SHA256", clientCert.ComputeSha256Hash());
                    request.Headers.Add("X-SSL-Client-DN", clientCert.Subject.Replace(" ", string.Empty));
                }
                
                using var client = new HttpClient(clientHandler);

                _Logger.WriteRequest(request);

                var response = await client.SendAsync(request);

                _Logger.WriteResponse(response.StatusCode);
                _Logger.WriteResponseHeaders(response.Headers);

                // Handle response
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        // EFGS returns the string 'null' if there is no batch tag. We will represent this with an actual null.
                        var nextBatchTag = response.Headers.SafeGetValue("nextBatchTag");
                        nextBatchTag = nextBatchTag == "null" ? null : nextBatchTag;
                        return new HttpGetIksSuccessResult
                        {
                            //TODO errors if info not present
                            BatchTag = response.Headers.SafeGetValue("batchTag"),
                            NextBatchTag = nextBatchTag,
                            Content = await response.Content.ReadAsByteArrayAsync()
                        };
                    case HttpStatusCode.NotFound:
                        _Logger.WriteResponseNotFound();
                        return null;
                    case HttpStatusCode.Gone:
                        _Logger.WriteResponseGone();
                        return null;
                    case HttpStatusCode.BadRequest:
                        _Logger.WriteResponseBadRequest();
                        throw new EfgsCommunicationException();
                    case HttpStatusCode.Forbidden:
                        _Logger.WriteResponseForbidden();
                        throw new EfgsCommunicationException();
                    case HttpStatusCode.NotAcceptable:
                        _Logger.WriteResponseNotAcceptable();
                        throw new EfgsCommunicationException();
                    default:
                        _Logger.WriteResponseUndefined(response.StatusCode);
                        throw new EfgsCommunicationException();
                }
            }
            catch (Exception e)
            {
                _Logger.WriteEfgsError(e);

                throw;
            }
        }
    }
}