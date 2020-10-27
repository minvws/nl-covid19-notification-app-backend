// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredEks
{
	public static class LoggingExtensionsExpiredEks
	{
		public static void WriteStart(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Begin removing expired EKS.",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.Start);
		}

		public static void WriteFinished(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Finished EKS cleanup.",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.Finished);
		}

		public static void WriteFinishedNothingRemoved(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Finished EKS cleanup. In safe mode - no deletions.",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.FinishedNothingRemoved);
		}

		public static void WriteCurrentEksFound(this ILogger logger, int totalFound)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Current EKS - Count:{found}.",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.CurrentEksFound,
				totalFound);
		}

		public static void WriteTotalEksFound(this ILogger logger, DateTime cutoff, int zombiesFound)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Found expired EKS - Cutoff:{cutoff:yyyy-MM-dd}, Count:{count}",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.FoundTotal,
				cutoff, zombiesFound);
		}

		public static void WriteEntryFound(this ILogger logger, string publishingId, DateTime releaseDate)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Found expired EKS - PublishingId:{PublishingId} Release:{Release}",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.FoundEntry,
				publishingId, releaseDate);
		}

		public static void WriteRemovedAmount(this ILogger logger, int givenMercy, int remaining)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Removed expired EKS - Count:{count}, Remaining:{remaining}",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.RemovedAmount,
				givenMercy, remaining);
		}

		public static void WriteReconciliationFailed(this ILogger logger, int reconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Reconciliation failed - Found-GivenMercy-Remaining:{remaining}.",
				LoggingDataRemoveExpiredEks.Name, LoggingDataRemoveExpiredEks.ReconciliationFailed,
				reconciliationCount);
		}
	}
}
