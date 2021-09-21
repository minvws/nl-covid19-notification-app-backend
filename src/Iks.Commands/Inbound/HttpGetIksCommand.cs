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
    public class HttpGetIksCommand : IHttpGetIksCommand
    {
        private const string ApplicationProtobuf = "application/protobuf; version=1.0";

        private readonly IEfgsConfig _efgsConfig;
        private readonly IAuthenticationCertificateProvider _certificateProvider;
        private readonly IksDownloaderLoggingExtensions _logger;
        private readonly HttpClient _httpClient;

        public HttpGetIksCommand(IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, IksDownloaderLoggingExtensions logger, HttpClientHandler httpClientHandler)
        {
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _certificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient(httpClientHandler ?? throw new ArgumentNullException(nameof(httpClientHandler)));
        }

        public async Task<HttpGetIksResult> ExecuteAsync(DateTime date, string batchTag)
        {
            try
            {
                _logger.WriteRequestingData(date, batchTag);

                var request = BuildHttpRequest(batchTag, date);

                _logger.WriteRequest(request);

                var response = await _httpClient.SendAsync(request);

                _logger.WriteResponse(response.StatusCode);
                _logger.WriteResponseHeaders(response.Headers);

                return await HandleResponse(response);
            }
            catch (Exception e)
            {
                _logger.WriteEfgsError(e);

                throw;
            }
        }

        private async Task<HttpGetIksResult> HandleResponse(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                // EFGS returns the string 'null' if there is no batch tag. We will represent this with an actual null.
                var nextBatchTag = response.Headers.SafeGetValue("nextBatchTag");
                nextBatchTag = nextBatchTag == "null" ? null : nextBatchTag;

                return new HttpGetIksResult
                {
                    //TODO errors if info not present
                    BatchTag = response.Headers.SafeGetValue("batchTag"),
                    NextBatchTag = nextBatchTag,
                    Content = await response.Content.ReadAsByteArrayAsync(),
                    ResultCode = response.StatusCode
                };
            }
            else
            {
                return new HttpGetIksResult
                {
                    BatchTag = string.Empty,
                    NextBatchTag = null,
                    ResultCode = response.StatusCode
                };
            }
        }

        private HttpRequestMessage BuildHttpRequest(string batchTag, DateTime date)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"{_efgsConfig.BaseUrl}/diagnosiskeys/download/{date:yyyy-MM-dd}", UriKind.RelativeOrAbsolute)
            };

            request.Headers.Add("Accept", ApplicationProtobuf);

            if (!string.IsNullOrWhiteSpace(batchTag))
            {
                request.Headers.Add("batchTag", batchTag);
            }

            if (_efgsConfig.SendClientAuthenticationHeaders)
            {
                //Might cause scope errors...
                using var clientCert = _certificateProvider.GetCertificate();

                request.Headers.Add("X-SSL-Client-SHA256", clientCert.ComputeSha256Hash());
                request.Headers.Add("X-SSL-Client-DN", clientCert.Subject.Replace(" ", string.Empty));
            }

            return request;
        }
    }
}
