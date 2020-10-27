using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifestV2
{
	public static class LoggingExtensionsExpiredManifestV2
	{
		public static void WriteStart(this ILogger logger, int keepAliveCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Begin removing expired ManifestV2s - Keep Alive Count:{count}.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.Start,
				keepAliveCount);
		}

		public static void WriteFinished(this ILogger logger, int zombieCount, int givenMercedesCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Finished removing expired ManifestV2s - ExpectedCount:{count} ActualCount:{givenMercy}.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.Finished,
				zombieCount, givenMercedesCount);
		}

		public static void WriteFinishedNothingRemoved(this ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Finished removing expired ManifestV2s - Nothing to remove.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.FinishedNothingRemoved);
		}

		public static void WriteRemovingManifests(this ILogger logger, int zombieCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Removing expired ManifestV2s - Count:{count}.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.RemovingManifests,
				zombieCount);
		}

		public static void WriteRemovingEntry(this ILogger logger, string publishingId, DateTime releaseDate)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogInformation("[{name}/{id}] Removing expired ManifestV2 - PublishingId:{PublishingId} Release:{Release}.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.RemovingEntry,
				publishingId, releaseDate);
		}

		public static void WriteReconciliationFailed(this ILogger logger, int reconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Reconciliation failed removing expired ManifestV2s - Found-GivenMercy-Remaining={reconciliation}.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.ReconciliationFailed,
				reconciliationCount);
		}

		public static void WriteDeletionReconciliationFailed(this ILogger logger, int deleteReconciliationCount)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			logger.LogError("[{name}/{id}] Reconciliation failed removing expired ManifestV2s - Zombies-GivenMercy={deadReconciliation}.",
				LoggingDataRemoveExpiredManifestV2.Name, LoggingDataRemoveExpiredManifestV2.DeletionReconciliationFailed,
				deleteReconciliationCount);
		}
	}
}
