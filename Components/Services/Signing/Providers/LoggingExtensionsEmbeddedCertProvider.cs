// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EmbeddedCertProvider
{
    public static class LoggingExtensionsEmbeddedCertProvider
    {
        private static string Name = "EmbeddedResourceCertificateProvider";
        private const int Base = LoggingCodex.EmbeddedCertProvider;

        private const int Opening = Base + 1;
        private const int ResourceFound = Base + 2;
        private const int ResourceFail = Base + 3;

        public static void WriteOpening(this ILogger logger, string? name)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Opening resource: {ResName}.",
                Name, Opening,
                name);
        }

        public static void WriteResourceFound(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Resource found.",
                Name, ResourceFound);
        }

        public static void WriteResourceFail(this ILogger logger, Exception exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError(exception, "[{name}/{id}] Failed to get manifest resource stream.",
                Name, ResourceFail,
                exception);
        }
    }

}