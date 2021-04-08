// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using JWT.Exceptions;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands
{
    public static class LoggingExtensionsIccBackend
    {
        private const string Name = "IccBackend";
        private const int Base = LoggingCodex.IccBackend;

        private const int HttpFail = Base + 1;
        private const int EmptyResponseString = Base + 2;
        private const int TokenVerifyResult = Base + 3;
        private const int TokenRevokeSuccess = Base + 4;
        private const int TokenNotRevoked = Base + 5;
        private const int InsufficientRole = Base + 6;
        private const int TestJwtConstructed = Base + 7;
        private const int GeneratedToken = Base + 8;
        private const int MissingAuthHeader = Base + 9;
        private const int InvalidAuthHeader = Base + 10;
        private const int TokenExpTooLong = Base + 11;
        private const int TestJwtUsed = Base + 12;
        private const int InvalidTokenFormat = Base + 13;
        private const int InvalidTokenParts = Base + 14;
        private const int TokenExpired = Base + 15;
        private const int TokenSigInvalid = Base + 16;
        private const int TokenOtherError = Base + 17;
        private const int Redirecting = Base + 18;
        private const int AuthStart = Base + 19;
        private const int LabStart = Base + 20;
        private const int LogValidationError = Base + 21;
        private const int LogValidationInfo = Base + 22;
        private const int KeyReleaseWorkflowStateNotFound = Base + 23;
        private const int WritingNewPollToken = Base + 24;
        private const int DuplicatePollTokenFound = Base + 25;
        private const int PollTokenCommit = Base + 26;
        
        private const int PubTekStart = Base + 27;
        private const int PublishTekCommit = Base + 28;

        public static void WriteHttpFail(this ILogger logger, Uri? uri, HttpStatusCode status, string response)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] {RequestUri}: Failed HTTP: {ResponseStatusCode} - {ResponseString}.",
                Name, HttpFail,
                uri, status, response);
        }

        public static void WriteEmptyResponseString(this ILogger logger, Uri? uri, HttpStatusCode status, string response)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] {RequestUri}: Failed ResponseString is empty: {ResponseStatusCode} - {ResponseString}.",
                Name, EmptyResponseString,
                uri, status, response);
        }

        public static void WriteTokenVerifyResult(this ILogger logger, string response)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Positive token verify result {ResponseString}.",
                Name, TokenVerifyResult,
                response);
        }

        public static void WriteTokenRevokeSuccess(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Access Token successfully revoked.",
                Name, TokenRevokeSuccess);
        }

        public static void WriteTokenNotRevoked(this ILogger logger, string status)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] Access Token not revoked, statuscode {ResponseStatusCode}.",
                Name, TokenNotRevoked,
                status);
        }

        public static void WriteInsufficientRole(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] AccessDenied for login, insufficient role.",
                Name, InsufficientRole);
        }

        public static void WriteTestJwtConstructed(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] TestJwtGeneratorService Singleton constructed, generating test JWT now....",
                Name, TestJwtConstructed);
        }

        public static void WriteGeneratedToken(this ILogger logger, string token)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] {Token}.",
                Name, GeneratedToken,
                token);
        }

        public static void WriteMissingAuthHeader(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Missing authorization header.",
                Name, MissingAuthHeader);
        }

        public static void WriteInvalidAuthHeader(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Invalid authorization header.",
                Name, InvalidAuthHeader);
        }

        public static void WriteTokenExpTooLong(this ILogger logger, string lifetimeHours)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Token invalid, has longer exp. than configured {claimLifetimeHours} hrs.",
                Name, TokenExpTooLong,
                lifetimeHours);
        }

        public static void WriteTestJwtUsed(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] Test JWT Used for authorization!.",
                Name, TestJwtUsed);
        }

        public static void WriteInvalidTokenFormat(this ILogger logger, FormatException exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning(exception, "[{name}/{id}] Invalid jwt token, FormatException.",
                Name, InvalidTokenFormat);
        }

        public static void WriteInvalidTokenParts(this ILogger logger, InvalidTokenPartsException exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning(exception, "[{name}/{id}] Invalid jwt token, InvalidTokenPartsException.",
                Name, InvalidTokenParts);
        }

        public static void WriteTokenExpired(this ILogger logger, TokenExpiredException exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning(exception, "[{name}/{id}] Invalid jwt token, TokenExpiredException",
                Name, TokenExpired);
        }

        public static void WriteTokenSigInvalid(this ILogger logger, SignatureVerificationException exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning(exception, "[{name}/{id}] Invalid jwt token, SignatureVerificationException.",
                Name, TokenSigInvalid);
        }

        public static void WriteTokenOtherError(this ILogger logger, Exception exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError(exception, "[{name}/{id}] Invalid jwt token, Other error.",
                Name, TokenOtherError);
        }

        public static void WriteRedirecting(this ILogger logger, string currentHost)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Executing Auth.Redirect on Host {CurrentHost}.",
                Name, Redirecting,
                currentHost);
        }

        public static void WriteAuthStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] POST Auth/Token triggered.",
                Name, AuthStart);
        }

        public static void WriteLabStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] POST lab confirmation triggered.",
                Name, LabStart);
        }

        public static void WriteLogValidationError(this ILogger logger, string messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] {Messages}.",
                Name, LogValidationError,
                messages);
        }

        public static void WriteLogValidationInfo(this ILogger logger, string messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] {Messages}.",
                Name, LogValidationInfo,
                messages);
        }

        public static void WriteKeyReleaseWorkflowStateNotFound(this ILogger logger, string labId)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] KeyReleaseWorkflowState not found - LabConfirmationId:{LabConfirmationId}.",
                Name, KeyReleaseWorkflowStateNotFound,
                labId);
        }

        public static void WriteWritingNewPollToken(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Writing.",
                Name, WritingNewPollToken);
        }

        public static void WriteDuplicatePollTokenFound(this ILogger logger, int attemptCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] Duplicate PollToken found - attempt:{AttemptCount}.",
                Name, DuplicatePollTokenFound,
                attemptCount);
        }

        public static void WritePollTokenCommit(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Committed.",
                Name, PollTokenCommit);
        }

        public static void WritePubTekStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] PUT PubTEK triggered.",
                Name, PubTekStart);
        }

        public static void WriteWritingPublishTek(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Writing.",
                Name, PublishTekCommit);
        }
    }
}