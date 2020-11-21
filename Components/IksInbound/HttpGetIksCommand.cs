// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Efgs;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Providers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Helpers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.IksInbound
{
    public class HttpGetIksCommand {

        private readonly IEfgsConfig _EfgsConfig;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IAuthenticationCertificateProvider _CertificateProvider;
        private readonly ILogger<HttpGetIksCommand> _Logger;

        public HttpGetIksCommand(IEfgsConfig efgsConfig, IUtcDateTimeProvider dateTimeProvider, IAuthenticationCertificateProvider certificateProvider, ILogger<HttpGetIksCommand> logger)
        {
            _EfgsConfig = efgsConfig ?? throw new ArgumentNullException(nameof(efgsConfig));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _CertificateProvider = certificateProvider ?? throw new ArgumentNullException(nameof(certificateProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpGetIksResult> ExecuteAsync(string batchTag)
        {
            // TODO: the whole batch tag / next batch logic is weird. it's a linked list,
            // only to have a linked list you must know what the next item is. you can't with efgs.
            // IF the batch-tag acts as a "send me all batches after this one" filter, then it's fine
            // (though you don't wanna send next-batch-tag then!).

            // TODO: ask EFGS about this. Or test it.

            // TODO: if there are no batches downloaded yet TODAY, do not send a batch tag

            var date = _DateTimeProvider.Snapshot;

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
                request.Headers.Add("Accept", "application/json;version=1.0");
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

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // EFGS returns the string 'null' if there is no batch tag.
                    // We will represent this with an actual null.
                    var nextBatchTag = response.Headers.SafeGetValue("nextBatchTag");
                    nextBatchTag = nextBatchTag == "null" ? null : nextBatchTag;
                    _Logger.LogInformation("nextBatchTag: {nextBatchTag}", nextBatchTag);

                    return new HttpGetIksResult
                    {
                        HttpStatusCode = HttpStatusCode.OK,
                        SuccessInfo = new HttpGetIksSuccessResult
                        {
                            //TODO errors if info not present
                            BatchTag = response.Headers.SafeGetValue("batchTag"),
                            NextBatchTag = nextBatchTag,
                            Content = await response.Content.ReadAsByteArrayAsync()
                        }
                    };
                }

                //
                // TODO: review where this goes
                //
                // Properly log the various fail codes
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        _Logger.LogInformation("EFGS: No data found");
                        break;
                    case HttpStatusCode.Gone:
                        _Logger.LogInformation("EFGS: No data found (expired)");
                        break;
                    case HttpStatusCode.BadRequest:
                        _Logger.LogCritical("EFGS: missing or invalid header!");
                        break;
                    case HttpStatusCode.Forbidden:
                        _Logger.LogCritical("EFGS: missing or invalid certificate!");
                        break;
                    case HttpStatusCode.NotAcceptable:
                        _Logger.LogCritical("EFGS: data format or content is not valid!");
                        break;
                    default:
                        _Logger.LogCritical("EFGS: undefined HTTP status returned!");
                        break;
                }

                return new HttpGetIksResult { HttpStatusCode = response.StatusCode };
            }
            catch (Exception e)
            {
                _Logger.LogError("EFGS error: {Message}", e.Message);

                return new HttpGetIksResult { Exception = true };
            }
        }
    }
}