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
    public class HttpGetIksCommand : IiHttpGetIksCommand
    {

        const string ApplicationProtobuf = "application/protobuf; version=1.0";

        private readonly IEfgsConfig _efgsConfig;
        private readonly IAuthenticationCertificateProvider _certificateProvider;
        private readonly IksDownloaderLoggingExtensions _logger;

        public HttpGetIksCommand(IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, IksDownloaderLoggingExtensions logger)
        {
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _certificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpGetIksSuccessResult> ExecuteAsync(string batchTag, DateTime date)
        {
            _logger.WriteProcessingData(date, batchTag);

            var uri = new Uri($"{_efgsConfig.BaseUrl}/diagnosiskeys/download/{date:yyyy-MM-dd}");

            try
            {
                // Configure authentication certificate
                using var clientCert = _certificateProvider.GetCertificate();
                using var clientHandler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual
                };

                // Provide the authentication certificate manually
                clientHandler.ClientCertificates.Clear();
                clientHandler.ClientCertificates.Add(clientCert);

                // Build the request
                var request = new HttpRequestMessage { RequestUri = uri };
                request.Headers.Add("Accept", ApplicationProtobuf);
                if (!string.IsNullOrWhiteSpace(batchTag))
                    request.Headers.Add("batchTag", batchTag);
                if (_efgsConfig.SendClientAuthenticationHeaders)
                {
                    request.Headers.Add("X-SSL-Client-SHA256", clientCert.ComputeSha256Hash());
                    request.Headers.Add("X-SSL-Client-DN", clientCert.Subject.Replace(" ", string.Empty));
                }

                using var client = new HttpClient(clientHandler);

                _logger.WriteRequest(request);

                var response = await client.SendAsync(request);

                _logger.WriteResponse(response.StatusCode);
                _logger.WriteResponseHeaders(response.Headers);

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
                        _logger.WriteResponseNotFound();
                        return null;
                    case HttpStatusCode.Gone:
                        _logger.WriteResponseGone();
                        return null;
                    case HttpStatusCode.BadRequest:
                        _logger.WriteResponseBadRequest();
                        throw new EfgsCommunicationException();
                    case HttpStatusCode.Forbidden:
                        _logger.WriteResponseForbidden();
                        throw new EfgsCommunicationException();
                    case HttpStatusCode.NotAcceptable:
                        _logger.WriteResponseNotAcceptable();
                        throw new EfgsCommunicationException();
                    default:
                        _logger.WriteResponseUndefined(response.StatusCode);
                        throw new EfgsCommunicationException();
                }
            }
            catch (Exception e)
            {
                _logger.WriteEfgsError(e);

                throw;
            }
        }
    }
}
