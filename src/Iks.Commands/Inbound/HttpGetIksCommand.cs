// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using Iks.Protobuf;
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
        private readonly IThumbprintConfig _thumbprintConfig;
        private readonly IksDownloaderLoggingExtensions _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpGetIksCommand(
            IEfgsConfig efgsConfig,
            IAuthenticationCertificateProvider certificateProvider,
            IThumbprintConfig thumbprintConfig,
            IHttpClientFactory httpClientFactory,
            IksDownloaderLoggingExtensions logger)
        {
            _efgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _certificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _thumbprintConfig = thumbprintConfig ?? throw new ArgumentNullException(nameof(thumbprintConfig));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpGetIksResult> ExecuteAsync(DateTime date, string batchTag)
        {
            try
            {
                _logger.WriteRequestingData(date, batchTag);

                var request = BuildHttpRequest(batchTag, date);

                _logger.WriteRequest(request);

                var httpClient = _httpClientFactory.CreateClient(UniversalConstants.EfgsDownloader);
                var response = await httpClient.SendAsync(request);

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
                var batchTag = response.Headers.SafeGetValue("batchTag");
                var nextBatchTag = response.Headers.SafeGetValue("nextBatchTag");
                nextBatchTag = nextBatchTag == "null" ? null : nextBatchTag;

                //var myresult = response.Content.ReadAsStringAsync().Result.Replace("\"", string.Empty);
                //var mybytearray = Convert.FromBase64String(myresult);

                if (!string.IsNullOrEmpty(batchTag) && TryParse(await response.Content.ReadAsByteArrayAsync(), out var parsedContent))
                {
                    return new HttpGetIksResult
                    {
                        BatchTag = batchTag,
                        NextBatchTag = nextBatchTag,
                        Content = parsedContent.ToByteArray(),
                        ResultCode = response.StatusCode
                    };
                }

                _logger.WriteBatchTagNotFound();
            }

            return new HttpGetIksResult
            {
                BatchTag = string.Empty,
                NextBatchTag = null,
                ResultCode = response.StatusCode
            };
        }
              

        private bool TryParse(byte[] buffer, out DiagnosisKeyBatch result)
        {
            result = null;
            try
            {
                var parser = new MessageParser<DiagnosisKeyBatch>(() => new DiagnosisKeyBatch());
                result = parser.ParseFrom(buffer);
                return true;
            }
            catch (InvalidProtocolBufferException e)
            {
                return false;
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
                using var clientCert = _certificateProvider.GetCertificate(
                    _thumbprintConfig.Thumbprint, _thumbprintConfig.RootTrusted);

                request.Headers.Add("X-SSL-Client-SHA256", clientCert.ComputeSha256Hash());
                request.Headers.Add("X-SSL-Client-DN", clientCert.Subject.Replace(" ", string.Empty));
            }

            return request;
        }
    }
}
