using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifest
{
	public static class LoggingExtensionsExpiredManifest
	{
		public static void WriteStart(this ILogger logger, int keepAliveCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Begin removing expired Manifests - Keep Alive Count:{count}.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.Start,
				keepAliveCount);
		}

		public static void WriteFinished(this ILogger logger, int zombieCount, int givenMercyCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Finished removing expired Manifests - ExpectedCount:{count} ActualCount:{givenMercy}.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.Finished,
				zombieCount, givenMercyCount);
		}

		public static void WriteFinishedNothingRemoved(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Finished removing expired Manifests - Nothing to remove.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.FinishedNothingRemoved);
		}

		public static void WriteRemovingManifests(this ILogger logger, int zombieCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Removing expired Manifests - Count:{count}.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.RemovingManifests,
				zombieCount);
		}

		public static void WriteRemovingEntry(this ILogger logger, string publishingId, DateTime releaseDate)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Removing expired Manifest - PublishingId:{PublishingId} Release:{Release}.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.RemovingEntry,
				publishingId, releaseDate);
		}

		public static void WriteReconcilliationFailed(this ILogger logger, int reconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Reconciliation failed removing expired Manifests - Found-GivenMercy-Remaining={reconciliation}.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.ReconciliationFailed,
				reconciliationCount);
		}

		public static void WriteDeletionReconciliationFailed(this ILogger logger, int deleteReconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Reconciliation failed removing expired Manifests - Zombies-GivenMercy={deadReconciliation}.",
				LoggingDataRemoveExpiredManifest.Name, LoggingDataRemoveExpiredManifest.DeletionReconciliationFailed,
				deleteReconciliationCount);
		}
	}
}
