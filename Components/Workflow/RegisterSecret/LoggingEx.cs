using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.RegisterSecret
{
    public static class LoggingEx
    {
        private const string Name = "Register";

        private static class Ids
        {
            public const int Start = LoggingIdScopeStart.Register;
            public const int Finished = LoggingIdScopeStart.Register + 99;

            public const int Writing = LoggingIdScopeStart.Register + 1;
            public const int Committed = LoggingIdScopeStart.Register + 2;
            public const int DuplicatesFound = LoggingIdScopeStart.Register + 3;

            public const int MaximumCreateAttemptsReached = LoggingIdScopeStart.Register + 4;
            public const int Failed = LoggingIdScopeStart.Register + 5;
        }

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] POST register triggered.", Name, Ids.Start);
        }

        public static void WriteFailed(this ILogger logger, Exception ex)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            logger.LogCritical(ex, "[{name}/{id}] Failed to create an enrollment response.", Name, Ids.Failed);
        }
        
        public static void WriteWritingStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Writing.", Name, Ids.Writing);
        }
        
        public static void WriteDuplicatesFound(this ILogger logger, int attemptCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogWarning("[{name}/{id}] Duplicates found while creating workflow - Attempt:{AttemptCount}", Name, attemptCount, Ids.DuplicatesFound);
        }
        
        public static void WriteMaximumCreateAttemptsReached(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogCritical("[{name}/{id}] Maximum attempts made at creating workflow.", Name, Ids.MaximumCreateAttemptsReached);
        }

        public static void WriteCommitted(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Committed.", Name, Ids.Committed);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Finished.", Name, Ids.Finished);
        }
    }
}
