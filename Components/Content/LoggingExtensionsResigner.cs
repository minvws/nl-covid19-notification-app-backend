// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.Resigner
{
    public static class LoggingExtensionsResigner
    {
        private const string Name = "Resigner";
        private const int First = LoggingCodex.Resigner;

        private const int CertNotSpecified = First;
        private const int Report = First + 1;
        private const int Finished = First + 99;

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Re-signing complete.",
                Name, Finished);
        }

        public static void WriteCertNotSpecified(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogWarning("[{name}/{id}] Certificate for re-signing not specified in settings. Re-signing will not run.",
                Name, CertNotSpecified);
        }

        public static void WriteReport(this ILogger logger, string? report)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] {report}.",
                Name, Report,
                report);
        }
    }
}
