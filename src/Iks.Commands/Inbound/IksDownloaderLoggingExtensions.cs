// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Inbound
{
    public class IksDownloaderLoggingExtensions
    {
        private const string Name = "IksDownloader";
        private const int Base = LoggingCodex.IksDownloader;

        private const int DisabledByConfig = Base + 1;
        private const int ProcessingData = Base + 2;
        private const int Request = Base + 3;
        private const int Response = Base + 4;
        private const int ResponseHeaders = Base + 5;
        private const int ResponseNotFound = Base + 6;
        private const int ResponseGone = Base + 7;
        private const int ResponseBadRequest = Base + 8;
        private const int ResponseForbidden = Base + 9;
        private const int ResponseNotAcceptable = Base + 10;
        private const int ResponseUndefined = Base + 11;
        private const int EfgsError = Base + 12;

        private const int BatchMaximumReached = Base + 13;
        private const int MovingToNextDay = Base + 17;
        private const int NoNextBatch = Base + 18;
        private const int NoNextBatchNoNextDay = Base + 20;
        private const int RequestingData = Base + 21;

        private readonly ILogger _logger;

        public IksDownloaderLoggingExtensions(ILogger<IksDownloaderLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteDisabledByConfig()
        {
            _logger.LogWarning("[{name}/{id}] EfgsDownloader is disabled by the configuration.",
                Name, DisabledByConfig);
        }

        public void WriteProcessingData(DateTime date, string batchtag)
        {
            _logger.LogInformation("[{name}/{id}] Processing data for {date}, batch {batchTag}",
                Name, ProcessingData,
                date, batchtag);
        }

        public void WriteRequestingData(DateTime date, string batchTag)
        {
            _logger.LogInformation("[{name}/{id}] Requesting data from EFGS for {date}, batch {batchTag}",
                Name, RequestingData,
                date, batchTag);
        }

        public void WriteRequest(HttpRequestMessage requestMessage)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException(nameof(requestMessage));
            }

            _logger.LogInformation("[{name}/{id}] EFGS request: {request}",
                Name, Request,
                requestMessage);
        }

        public void WriteResponse(HttpStatusCode statusCode)
        {
            _logger.LogInformation("[{name}/{id}] Response from EFGS: {statusCodeInt} {statuscode}.",
                Name, Response,
                (int)statusCode, statusCode);
        }

        public void WriteResponseHeaders(HttpResponseHeaders responseHeaders)
        {
            if (responseHeaders == null)
            {
                throw new ArgumentNullException(nameof(responseHeaders));
            }

            _logger.LogInformation("[{name}/{id}] Response headers: {headers}",
                Name, ResponseHeaders,
                responseHeaders.ToString());
        }

        public void WriteResponseNotFound()
        {
            _logger.LogWarning("[{name}/{id}] EFGS: No data found.",
                Name, ResponseNotFound);
        }

        public void WriteResponseGone()
        {
            _logger.LogWarning("[{name}/{id}] EFGS: No data found (expired).",
                Name, ResponseGone);
        }

        public void WriteResponseBadRequest()
        {
            _logger.LogCritical("[{name}/{id}] EFGS: missing or invalid header!",
                Name, ResponseBadRequest);
        }

        public void WriteResponseForbidden()
        {
            _logger.LogCritical("[{name}/{id}] EFGS: missing or invalid certificate!",
                Name, ResponseForbidden);
        }

        public void WriteResponseNotAcceptable()
        {
            _logger.LogCritical("[{name}/{id}] EFGS: data format or content is not valid!",
                Name, ResponseNotAcceptable);
        }

        public void WriteResponseUndefined(HttpStatusCode statusCode)
        {
            _logger.LogCritical("[{name}/{id}] EFGS: undefined HTTP status ({status}) returned!",
                Name, ResponseUndefined,
                statusCode);
        }

        public void WriteEfgsError(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _logger.LogCritical("[{name}/{id}] EFGS error: {Message}",
                Name, EfgsError,
                exception.Message);
        }

        public void WriteBatchMaximumReached(int maxBatchesPerRun)
        {
            _logger.LogInformation("[{name}/{id}] Maximum number of batches per run of {maxBatches} reached.",
                Name, BatchMaximumReached,
                maxBatchesPerRun);
        }

        public void WriteMovingToNextDay()
        {
            _logger.LogInformation("[{name}/{id}] Moving to the next day!",
                Name, MovingToNextDay);
        }

        public void WriteNoNextBatch()
        {
            _logger.LogInformation("[{name}/{id}] No next batch, so: ending this day.",
                Name, NoNextBatch);
        }

        public void WriteNoNextBatchNoMoreDays()
        {
            _logger.LogInformation("[{name}/{id}] No next batch, no more available days, so: ending.",
            Name, NoNextBatchNoNextDay);
        }
    }
}
