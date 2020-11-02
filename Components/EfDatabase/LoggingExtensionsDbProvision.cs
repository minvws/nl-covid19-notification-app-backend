// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.DbProvision
{
    public static class LoggingExtensionsDbProvision
    {
        private const string Name = "DbProvision";
        private const int Base = LoggingCodex.DbProvision;
        private const int Start = Base;
        private const int Finished = Base + 99;
        private const int WorkflowDb = Base + 1;
        private const int ContentDb = Base + 2;
        private const int JobDb = Base + 3;

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Start.",
                Name, Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Complete.",
                Name, Finished);
        }

        public static void WriteWorkFlowDb(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Workflow...",
                Name, WorkflowDb);
        }

        public static void WriteContentDb(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Content...",
                Name, ContentDb);
        }

        public static void WriteJobDb(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            logger.LogInformation("[{name}/{id}] Job...",
                Name, JobDb);
        }
    }
}
