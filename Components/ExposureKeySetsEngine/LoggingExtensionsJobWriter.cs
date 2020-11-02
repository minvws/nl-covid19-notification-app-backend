// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.EksJobContentWriter
{
    public static class LoggingExtensionsEksJobContentWriter
    {
        private const string Name = "EksJobContentWriter";
        private const int Base = LoggingCodex.EksJobContentWriter;
        private const int Published = Base;

        public static void WritePublished(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Published EKSs - Count:{Count}.",
                Name, Published,
                count);
        }
    }
}
