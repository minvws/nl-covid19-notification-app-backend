// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredWorkflow
{
	public static class LoggingExtensionsExpiredWorkflow
	{
		public static void WriteStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Begin Workflow cleanup.",
				LoggingDataRemoveExpiredWorkflow.Name, LoggingDataRemoveExpiredWorkflow.Start);
		}

		public static void WriteFinished(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Workflow cleanup complete.",
				LoggingDataRemoveExpiredWorkflow.Name, LoggingDataRemoveExpiredWorkflow.Finished);
		}

		public static void WriteFinishedNothingRemoved(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] No Workflows deleted - Deletions switched off.",
				LoggingDataRemoveExpiredWorkflow.Name, LoggingDataRemoveExpiredWorkflow.FinishedNothingRemoved);
		}

		public static void WriteReport(this ILogger logger, string report)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] {report}.",
				LoggingDataRemoveExpiredWorkflow.Name, LoggingDataRemoveExpiredWorkflow.Report,
				report);
		}

		public static void WriteUnpublishedTekFound(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogCritical("[{name}/{id}] Authorised unpublished TEKs exist. Aborting workflow cleanup.",
				LoggingDataRemoveExpiredWorkflow.Name, LoggingDataRemoveExpiredWorkflow.UnpublishedTekFound);
		}

		public static void WriteRemovedAmount(this ILogger logger, int givenMercyCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Workflows deleted - Unauthorised:{unauthorised}",
				LoggingDataRemoveExpiredWorkflow.Name, LoggingDataRemoveExpiredWorkflow.RemovedAmount,
				givenMercyCount);
		}
	}
}
