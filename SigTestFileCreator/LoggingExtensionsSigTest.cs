// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.SigTestFileCreator
{
    public static class LoggingExtensionsSigTestFileCreator
    {
        private const string Name = "SigTestFileCreator";
        private const int Base = LoggingCodex.SigtestFileCreator;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int NoElevatedPrivs = Base + 1;
        private const int BuildingResultFile = Base + 2;
        private const int SavingResultFile = Base + 3;


        public static void WriteStart(this ILogger logger, DateTime time)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Key presence Test started ({Time}).",
                Name, Start,
                time);
        }

        public static void WriteFinished(this ILogger logger, string outputLocation)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Key presence test complete.\nResults can be found in: {OutputLocation}.",
                Name, Finished,
                outputLocation);
        }

        public static void WriteNoElevatedPrivs(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] The test was started WITHOUT elevated privileges - errors may occur when signing content.",
                Name, NoElevatedPrivs);
        }

        public static void WriteBuildingResultFile(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Building EKS-engine resultfile.",
                Name, BuildingResultFile);
        }

        public static void WriteSavingResultfile(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Saving EKS-engine resultfile.",
                Name, SavingResultFile);
        }
    }

}
