// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.IccBackend
{
	public static class LoggingExtensionsIccBackend
	{
		public static void WriteHttpFail(this ILogger logger, Uri? uri, HttpStatusCode status, string response)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] {RequestUri}: Failed HTTP: {ResponseStatusCode} - {ResponseString}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.HttpFail, 
				uri, status, response);
		}

		public static void WriteEmptyResponseString(this ILogger logger, Uri? uri, HttpStatusCode status, string response)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] {RequestUri}: Failed ResponseString is empty: {ResponseStatusCode} - {ResponseString}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.EmptyResponseString, 
				uri, status, response);
		}

		public static void WriteTokenVerifyResult(this ILogger logger, string response)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Positive token verify result {ResponseString}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenVerifyResult, 
				response);
		}

		public static void WriteTokenRevokeSuccess(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Access Token successfully revoked.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenRevokeSuccess);
		}

		public static void WriteTokenNotRevoked(this ILogger logger, string status)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Access Token not revoked, statuscode {ResponseStatusCode}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenNotRevoked, 
				status);
		}

		public static void WriteInsufficientRole(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] AccessDenied for login, insufficient role.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.InsufficientRole);
		}

		public static void WriteTestJwtConstructed(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] TestJwtGeneratorService Singleton constructed, generating test JWT now....",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TestJwtConstructed);
		}

		public static void WriteGeneratedToken(this ILogger logger, string token)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] {Token}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.GeneratedToken,
				token);
		}

		public static void WriteMissingAuthHeader(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Missing authorization header.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.MissingAuthHeader);
		}

		public static void WriteInvalidAuthHeader(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Invalid authorization header.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.InvalidAuthHeader);
		}

		public static void WriteTokenExpTooLong(this ILogger logger, string lifetimeHours)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Token invalid, has longer exp. than configured {claimLifetimeHours} hrs.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenExpTooLong,
				lifetimeHours);
		}

		public static void WriteTestJwtUsed(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Test JWT Used for authorization!.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TestJwtUsed);
		}

		public static void WriteInvalidTokenFormat(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Invalid jwt token, FormatException.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.InvalidTokenFormat);
		}

		public static void WriteInvalidTokenParts(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Invalid jwt token, InvalidTokenPartsException.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.InvalidTokenParts);
		}

		public static void WriteTokenExpired(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Invalid jwt token, TokenExpiredException",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenExpired);
		}

		public static void WriteTokenSigInvalid(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Invalid jwt token, SignatureVerificationException.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenSigInvalid);
		}

		public static void WriteTokenOtherError(this ILogger logger, Exception exception)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError(exception, "[{name}/{id}] Invalid jwt token, Other error.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.TokenOtherError);
		}

		public static void WriteRedirecting(this ILogger logger, string currentHost)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Executing Auth.Redirect on Host {CurrentHost}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.Redirecting,
				currentHost);
		}

		public static void WriteAuthStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] POST Auth/Token triggered.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.AuthStart);
		}

		public static void WriteLabStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] POST lab confirmation triggered.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.LabStart);
		}

		public static void WriteLogValidationError(this ILogger logger, string messages)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] {Messages}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.LogValidationError,
				messages);
		}

		public static void WriteLogValidationInfo(this ILogger logger, string messages)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] {Messages}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.LogValidationInfo, 
				messages);
		}

		public static void WriteKeyReleaseWorkflowStateNotFound(this ILogger logger, string labId)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] KeyReleaseWorkflowState not found - LabConfirmationId:{LabConfirmationId}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.KeyReleaseWorkflowStateNotFound, 
				labId);
		}

		public static void WriteWritingNewPollToken(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Writing.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.WritingNewPollToken);
		}

		public static void WriteDuplicatePollTokenFound(this ILogger logger, int attemptCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogWarning("[{name}/{id}] Duplicate PollToken found - attempt:{AttemptCount}.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.DuplicatePollTokenFound, 
				attemptCount);
		}

		public static void WritePollTokenCommit(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogDebug("[{name}/{id}] Committed.",
				LoggingDataIccBackend.Name, LoggingDataIccBackend.PollTokenCommit);
		}

	}
}