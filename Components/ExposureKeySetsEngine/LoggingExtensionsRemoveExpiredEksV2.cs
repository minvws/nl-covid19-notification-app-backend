// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredEksV2
{
    public static class LoggingExtensionsExpiredEksV2
    {
        private const string Name = "RemoveExpiredEksV2";
        private const int Base = LoggingCodex.RemoveExpiredEksV2;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int CurrentEksFound = Base + 1;
        private const int FoundTotal = Base + 2;
        private const int FoundEntry = Base + 3;
        private const int RemovedAmount = Base + 4;
        private const int ReconciliationFailed = Base + 5;
        private const int FinishedNothingRemoved = Base + 98;

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Begin removing expired EKSv2.",
                Name, Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finished EKSv2 cleanup.",
                Name, Finished);
        }

        public static void WriteFinishedNothingRemoved(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Finished EKSv2 cleanup. In safe mode - no deletions.",
                Name, FinishedNothingRemoved);
        }

        public static void WriteCurrentEksFound(this ILogger logger, int totalFound)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Current EKS - Count:{found}.",
                Name, CurrentEksFound,
                totalFound);
        }

        public static void WriteTotalEksFound(this ILogger logger, DateTime cutoff, int zombiesFound)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Found expired EKSv2 - Cutoff:{cutoff:yyyy-MM-dd}, Count:{count}",
                Name, FoundTotal,
                cutoff, zombiesFound);
        }

        public static void WriteEntryFound(this ILogger logger, string publishingId, DateTime releaseDate)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Found expired EKSv2 - PublishingId:{PublishingId} Release:{Release}",
                Name, FoundEntry,
                publishingId, releaseDate);
        }

        public static void WriteRemovedAmount(this ILogger logger, int givenMercy, int remaining)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Removed expired EKSv2 - Count:{count}, Remaining:{remaining}",
                Name, RemovedAmount,
                givenMercy, remaining);
        }

        public static void WriteReconciliationFailed(this ILogger logger, int reconciliationCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] Reconciliation failed - Found-GivenMercy-Remaining:{remaining}.",
                Name, ReconciliationFailed,
                reconciliationCount);
        }
    }
}
