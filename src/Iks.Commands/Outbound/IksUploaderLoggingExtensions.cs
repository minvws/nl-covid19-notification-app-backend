// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Uploader.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound
{
    public class IksUploaderLoggingExtensions
    {
        private const string Name = "IksUploader";
        private const int Base = LoggingCodex.IksUploader;

        private const int DisabledByConfig = Base + 1;

        private const int Request = Base + 2;
        private const int RequestContent = Base + 3;
        private const int EfgsError = Base + 4;
        private const int EfgsErrorInnerException = Base + 5;

        private const int ResponseSuccess = Base + 6;
        private const int ResponseWithWarnings = Base + 7;
        private const int ResponseBadRequest = Base + 8;
        private const int ResponseForbidden = Base + 9;
        private const int ResponseNotAcceptable = Base + 10;
        private const int ResponseRequestTooLarge = Base + 11;
        private const int ResponseServerError = Base + 12;
        private const int ResponseUnknownError = Base + 13;
        private const int BatchNotExisting = Base + 14;

        private readonly ILogger _logger;

        public IksUploaderLoggingExtensions(ILogger<IksUploaderLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteDisabledByConfig()
        {
            _logger.LogWarning("[{name}/{id}] EfgsUploader is disabled by the configuration.",
                Name, DisabledByConfig);
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

        public void WriteRequestContent(byte[] content)
        {
            _logger.LogDebug("[{name}/{id}] EFGS request content: {content}",
                Name, RequestContent,
                Convert.ToBase64String(content));
        }

        public void WriteEfgsError(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception.StackTrace == null)
            {
                throw new ArgumentNullException(nameof(exception.StackTrace));
            }

            _logger.LogError("[{name}/{id}] Error calling EFGS, see exception for details.\n{errorMessage}\n{stackTrace}",
                Name, EfgsError,
                exception.Message, exception.StackTrace);
        }

        public void WriteEfgsInnerException(Exception innerexception)
        {
            if (innerexception == null)
            {
                throw new ArgumentNullException(nameof(innerexception));
            }

            if (innerexception.StackTrace == null)
            {
                throw new ArgumentNullException(nameof(innerexception.StackTrace));
            }

            _logger.LogError("[{name}/{id}] Inner exception:\n{errorMessage}\n{stackTrace}",
                Name, EfgsErrorInnerException,
                innerexception.Message, innerexception.StackTrace);
        }

        public void WriteResponseSuccess()
        {
            _logger.LogInformation("[{name}/{id}] EFGS: Success.",
                Name, ResponseSuccess);
        }

        public void WriteResponseWithWarnings(string responseContent)
        {
            if (responseContent == null)
            {
                throw new ArgumentNullException(nameof(responseContent));
            }

            _logger.LogWarning("[{name}/{id}] EFGS: Successful but with warnings: {content}",
                Name, ResponseWithWarnings,
                responseContent);
        }

        public void WriteResponseBadRequest()
        {
            _logger.LogError("[{name}/{id}] EFGS: Invalid request (either errors in the data or an invalid signature).",
                Name, ResponseBadRequest);
        }

        public void WriteResponseForbidden()
        {
            _logger.LogError("[{name}/{id}] EFGS: Invalid/missing certificates.",
                Name, ResponseForbidden);
        }

        public void WriteResponseNotAcceptable()
        {
            _logger.LogError("[{name}/{id}] EFGS:  Data format or content is not valid.",
                Name, ResponseNotAcceptable);
        }

        public void WriteResponseRequestTooLarge()
        {
            _logger.LogError("[{name}/{id}] EFGS: Data already exists (Http: Request Entity too large).",
                Name, ResponseRequestTooLarge);
        }

        public void WriteResponseInternalServerError()
        {
            _logger.LogError("[{name}/{id}] EFGS: Not able to write data. Retry please.",
                Name, ResponseServerError);
        }

        public void WriteResponseUnknownError(HttpStatusCode? statusCode)
        {
            if (statusCode == null)
            {
                throw new ArgumentNullException(nameof(statusCode));
            }

            _logger.LogError("[{name}/{id}] Unknown error: {httpResponseCode}.",
                Name, ResponseUnknownError,
                statusCode);
        }

        public void WriteBatchNotExistInEntity(IksOutEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _logger.LogError("[{name}/{id}] Batch does not exist in entity with Id: {Id}",
                Name, BatchNotExisting,
                entity.Id);
        }
    }
}
