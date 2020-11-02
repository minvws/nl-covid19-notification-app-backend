// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ManifestUpdateCommand
{
    public static class LoggingExtensionsManifestUpdateCommand
    {
        private static string Name = "ManifestUpdateCommand";
        private const int Base = LoggingCodex.ManifestUpdate;
        private const int Start = Base;
        private const int Finished = Base + 99;
        private const int UpdateNotRequired = Base + 1;

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Manifest updating.",
                Name, Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Manifest updated.",
                Name, Finished);
        }

        public static void WriteUpdateNotRequired(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Manifest does NOT require updating.",
                Name, UpdateNotRequired);
        }
    }
}