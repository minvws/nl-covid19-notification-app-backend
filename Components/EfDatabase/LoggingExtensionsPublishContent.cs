// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.PublishContent
{
    public static class LoggingExtensionsPublishContent
    {
        private const string Name = "PublishContent";
        private const int Base = LoggingCodex.PublishContent;

        private const int StartWriting = Base + 1;
        private const int FinishedWriting = Base + 2;

        public static void WriteStartWriting(this ILogger logger, string contentType)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Writing {ContentType} to database.",
                Name, StartWriting,
                contentType);
        }

        public static void WriteFinishedWriting(this ILogger logger, string contentType)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogDebug("[{name}/{id}] Done writing {ContentType} to database.",
                Name, FinishedWriting,
                contentType);
        }
    }
}