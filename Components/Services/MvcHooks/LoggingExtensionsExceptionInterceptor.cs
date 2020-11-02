// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.ExceptionInterceptor
{
    public static class LoggingExtensionsExceptionInterceptor
    {
        private const string Name = "MsLoggerServiceExceptionInterceptor";

        private const int ExceptionFound = LoggingCodex.ExceptionInterceptor;

        public static void WriteExceptionFound(this ILogger logger, string exception)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogError("[{name}/{id}] {Exception}", Name, ExceptionFound, exception);
        }
    }
}
