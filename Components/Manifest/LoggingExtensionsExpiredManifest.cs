using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredManifest
{
    public static class LoggingExtensionsExpiredManifest
    {
        private const string Name = "RemoveExpiredManifest";
        private const int First = LoggingCodex.RemoveExpiredManifest;

        private const int Start = First;
        private const int Finished = First + 99;

        private const int RemovingManifests = First + 1;
        private const int RemovingEntry = First + 2;
        private const int ReconciliationFailed = First + 3;
        private const int DeletionReconciliationFailed = First + 4;
        private const int FinishedNothingRemoved = First + 98;

        public static void WriteStart(this ILogger logger, int keepAliveCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Begin removing expired Manifests - Keep Alive Count:{count}.",
                Name, Start,
                keepAliveCount);
        }

        public static void WriteFinished(this ILogger logger, int zombieCount, int givenMercyCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finished removing expired Manifests - ExpectedCount:{count} ActualCount:{givenMercy}.",
                Name, Finished,
                zombieCount, givenMercyCount);
        }

        public static void WriteFinishedNothingRemoved(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finished removing expired Manifests - Nothing to remove.",
                Name, FinishedNothingRemoved);
        }

        public static void WriteRemovingManifests(this ILogger logger, int zombieCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Removing expired Manifests - Count:{count}.",
                Name, RemovingManifests,
                zombieCount);
        }

        public static void WriteRemovingEntry(this ILogger logger, string publishingId, DateTime releaseDate)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Removing expired Manifest - PublishingId:{PublishingId} Release:{Release}.",
                Name, RemovingEntry,
                publishingId, releaseDate);
        }

        public static void WriteReconcilliationFailed(this ILogger logger, int reconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] Reconciliation failed removing expired Manifests - Found-GivenMercy-Remaining={reconciliation}.",
                Name, ReconciliationFailed,
                reconciliationCount);
        }

        public static void WriteDeletionReconciliationFailed(this ILogger logger, int deleteReconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] Reconciliation failed removing expired Manifests - Zombies-GivenMercy={deadReconciliation}.",
                Name, DeletionReconciliationFailed,
                deleteReconciliationCount);
        }
    }
}
