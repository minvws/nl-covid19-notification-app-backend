// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExpiredWorkflow
{
    public static class LoggingExtensionsExpiredWorkflow
    {
        private const string Name = "RemoveExpiredWorkflow";
        private const int Base = LoggingCodex.RemoveExpiredWorkflow;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int Report = Base + 1;
        private const int RemovedAmount = Base + 2;
        private const int UnpublishedTekFound = Base + 97;
        private const int FinishedNothingRemoved = Base + 98;

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Begin Workflow cleanup.",
                Name, Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Workflow cleanup complete.",
                Name, Finished);
        }

        public static void WriteFinishedNothingRemoved(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] No Workflows deleted - Deletions switched off.",
                Name, FinishedNothingRemoved);
        }

        public static void WriteReport(this ILogger logger, string report)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] {report}.",
                Name, Report,
                report);
        }

        public static void WriteUnpublishedTekFound(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogCritical("[{name}/{id}] Authorised unpublished TEKs exist. Aborting workflow cleanup.",
                Name, UnpublishedTekFound);
        }

        public static void WriteRemovedAmount(this ILogger logger, int givenMercyCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Workflows deleted - Unauthorised:{unauthorised}",
                Name, RemovedAmount,
                givenMercyCount);
        }
    }
}
