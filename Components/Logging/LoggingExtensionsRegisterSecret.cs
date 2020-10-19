using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.RegisterSecret
{
    public static class LoggingExtensionsRegisterSecret
    {
        public static void WriteStartSecret(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] POST register triggered.", LoggingDataRegisterSecret.Name, LoggingDataRegisterSecret.Start);
        }

        public static void WriteFailed(this ILogger logger, Exception ex)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            logger.LogCritical(ex, "[{name}/{id}] Failed to create an enrollment response.", LoggingDataRegisterSecret.Name, LoggingDataRegisterSecret.Failed);
        }
        
        public static void WriteWritingStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Writing.", LoggingDataRegisterSecret.Name, LoggingDataRegisterSecret.Writing);
        }
        
        public static void WriteDuplicatesFound(this ILogger logger, int attemptCount)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogWarning("[{name}/{id}] Duplicates found while creating workflow - Attempt:{AttemptCount}", LoggingDataRegisterSecret.Name, attemptCount, LoggingDataRegisterSecret.DuplicatesFound);
        }
        
        public static void WriteMaximumCreateAttemptsReached(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogCritical("[{name}/{id}] Maximum attempts made at creating workflow.", LoggingDataRegisterSecret.Name, LoggingDataRegisterSecret.MaximumCreateAttemptsReached);
        }

        public static void WriteCommitted(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Committed.", LoggingDataRegisterSecret.Name, LoggingDataRegisterSecret.Committed);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Finished.", LoggingDataRegisterSecret.Name, LoggingDataRegisterSecret.Finished);
        }
    }
}
