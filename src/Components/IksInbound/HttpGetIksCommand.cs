// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Helpers;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
{
    public class EfgsCommunicationException : Exception
    {
    }

    public class HttpGetIksCommand : IIHttpGetIksCommand {

        const string ApplicationProtobuf = "application/protobuf; version=1.0";

        private readonly IEfgsConfig _EfgsConfig;
        private readonly IAuthenticationCertificateProvider _CertificateProvider;
        private readonly ILogger<HttpGetIksCommand> _Logger;

        public HttpGetIksCommand(IEfgsConfig efgsConfig, IAuthenticationCertificateProvider certificateProvider, ILogger<HttpGetIksCommand> logger)
        {
            _EfgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpGetIksSuccessResult?> ExecuteAsync(string batchTag, DateTime date)
        {
            _Logger.LogInformation("Processing data for {date}, batch {batchTag}", date, batchTag);

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

                _Logger.LogInformation("EFGS request:  {request}", request);

                var response = await client.SendAsync(request);

                _Logger.LogInformation("Response from EFGS: {0} {1}", (int)response.StatusCode, response.StatusCode);
                _Logger.LogInformation("Response headers: ", response.Headers.ToString());

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
                        _Logger.LogWarning("EFGS: No data found");
                        return null;
                    case HttpStatusCode.Gone:
                        _Logger.LogWarning("EFGS: No data found (expired)");
                        return null;
                    case HttpStatusCode.BadRequest:
                        _Logger.LogCritical("EFGS: missing or invalid header!");
                        throw new EfgsCommunicationException();
                    case HttpStatusCode.Forbidden:
                        _Logger.LogCritical("EFGS: missing or invalid certificate!");
                        throw new EfgsCommunicationException();
                    case HttpStatusCode.NotAcceptable:
                        _Logger.LogCritical("EFGS: data format or content is not valid!");
                        throw new EfgsCommunicationException();
                    default:
                        _Logger.LogCritical("EFGS: undefined HTTP status ({status}) returned!", response.StatusCode);
                        throw new EfgsCommunicationException();
                }
            }
            catch (Exception e)
            {
                _Logger.LogCritical("EFGS error: {Message}", e.Message);

                throw;
            }
        }
    }
}