// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DailyCleanup
{
    public static class LoggingExtensionsDailyCleanup
    {
        private const string Name = "DailyCleanup";
        private const int Base = LoggingCodex.DailyCleanup;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int EksEngineStarting = Base + 1;
        private const int ManifestEngineStarting = Base + 2;
        private const int DailyStatsCalcStarting = Base + 3;
        private const int ManiFestCleanupStarting = Base + 4;
        private const int EksCleanupStarting = Base + 5;
        private const int WorkflowCleanupStarting = Base + 6;
        private const int ResignerStarting = Base + 7;
        private const int EksV2CleanupStarting = Base + 8;
        private const int ManifestV2CleanupStarting = Base + 9;

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Starting.",
                Name, Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Finished.",
                Name, Finished);
        }

        public static void WriteEksEngineStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - EKS engine run starting.",
                Name, EksEngineStarting);
        }

        public static void WriteManifestEngineStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Manifest engine run starting.",
                Name, ManifestEngineStarting);
        }

        public static void WriteDailyStatsCalcStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Calculating daily stats starting.",
                Name, DailyStatsCalcStarting);
        }

        public static void WriteManiFestCleanupStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Manifests run starting.",
                Name, ManiFestCleanupStarting);
        }

        public static void WriteEksCleanupStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup EKS run starting.",
                Name, EksCleanupStarting);
        }

        public static void WriteWorkflowCleanupStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup Workflows run starting.",
                Name, WorkflowCleanupStarting);
        }

        public static void WriteResignerStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Resigning existing v1 content.",
                Name, ResignerStarting);
        }

        public static void WriteEksV2CleanupStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup EKSv2 run starting.",
                Name, EksV2CleanupStarting);
        }

        public static void WriteManifestV2CleanupStarting(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Daily cleanup - Cleanup ManifestV2 run starting.",
                Name, ManifestV2CleanupStarting);
        }
    }
}