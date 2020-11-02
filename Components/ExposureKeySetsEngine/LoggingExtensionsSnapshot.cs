// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Snapshot
{
    public static class LoggingExtensionsSnapshot
    {
        private const string Name = "Snapshot";

        private const int Base = LoggingCodex.Snapshot;
        private const int Start = Base;
        private const int TeksToPublish = Base + 1;

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Snapshot publishable TEKs..",
                Name, Start);
        }

        public static void WriteTeksToPublish(this ILogger logger, int tekCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] TEKs to publish - Count:{Count}.",
                Name, TeksToPublish,
                tekCount);
        }
    }
}