using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.RegisterSecret
{
    public static class LoggingExtensionsRegisterSecret
    {
        private const string Name = "Register";

        private const int Base = LoggingCodex.Register;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int Writing = Base + 1;
        private const int Committed = Base + 2;
        private const int DuplicatesFound = Base + 3;

        private const int MaximumCreateAttemptsReached = Base + 4;
        private const int Failed = Base + 5;

        public static void WriteStartSecret(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] POST register triggered.", Name, Start);
        }

        public static void WriteFailed(this ILogger logger, Exception ex)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            logger.LogCritical(ex, "[{name}/{id}] Failed to create an enrollment response.", Name, Failed);
        }
        
        public static void WriteWritingStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Writing.", Name, Writing);
        }
        
        public static void WriteDuplicatesFound(this ILogger logger, int attemptCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogWarning("[{name}/{id}] Duplicates found while creating workflow - Attempt:{AttemptCount}", Name, attemptCount, DuplicatesFound);
        }
        
        public static void WriteMaximumCreateAttemptsReached(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogCritical("[{name}/{id}] Maximum attempts made at creating workflow.", Name, MaximumCreateAttemptsReached);
        }

        public static void WriteCommitted(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Committed.", Name, Committed);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Finished.", Name, Finished);
        }
    }
}
