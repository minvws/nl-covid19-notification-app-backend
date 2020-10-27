using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.PostKeys
{
    public static class LoggingExtensionsPostKeys
    {
        public static void WriteStartPostKeys(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] POST triggered.", LoggingDataPostKeys.Name, LoggingDataPostKeys.Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Finished.", LoggingDataPostKeys.Name, LoggingDataPostKeys.Finished);
        }

        public static void WriteSignatureValidationFailed(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Signature is null or incorrect length.", LoggingDataPostKeys.Name, LoggingDataPostKeys.SignatureValidationFailed);
        }

        public static void WritePostBodyParsingFailed(this ILogger logger, Exception e)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation(e, "[{name}/{id}] Error reading body");
        }

        public static void WriteBucketIdParsingFailed(this ILogger logger, string input, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] BucketId failed validation - BucketId:{input} Messages:{messages}", LoggingDataPostKeys.Name, LoggingDataPostKeys.BucketIdParsingFailed, input, string.Join(",", messages));
        }

        public static void WriteTekValidationFailed(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:{messages}", LoggingDataPostKeys.Name, LoggingDataPostKeys.TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteTekDuplicatesFound(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Tek duplicates found - Messages:{messages}", LoggingDataPostKeys.Name, LoggingDataPostKeys.TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteApplicableWindowFilterResult(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:{messages}", LoggingDataPostKeys.Name, LoggingDataPostKeys.TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteValidTekCount(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] TEKs remaining - Count:{Count}.", LoggingDataPostKeys.Name, LoggingDataPostKeys.ValidTekCount, count);
        }

        public static void WriteBucketDoesNotExist(this ILogger logger, string bucketId)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogError("[{name}/{id}] Bucket does not exist - Id:{BucketId}.", LoggingDataPostKeys.Name, LoggingDataPostKeys.BucketDoesNotExist, bucketId); //_ArgsObject.BucketId
        }

        /// <summary>
        /// Base64 or Hex?
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="bucketId"></param>
        /// <param name="signature"></param>
        public static void WriteSignatureInvalid(this ILogger logger, byte[] bucketId, byte[] signature)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogError("[{name}/{id}] Signature not valid - Signature:{Signature} Bucket:{}", LoggingDataPostKeys.Name, LoggingDataPostKeys.SignatureInvalid, Convert.ToBase64String(signature), Convert.ToBase64String(bucketId));
        }

        public static void WriteWorkflowFilterResults(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] WriteWorkflowFilterResults - Count:{Count}.", LoggingDataPostKeys.Name, LoggingDataPostKeys.WorkflowFilterResults, string.Join(",", messages));
        }

        public static void WriteValidTekCountSecondPass(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] TEKs remaining 2 - Count:{Count}.", LoggingDataPostKeys.Name, LoggingDataPostKeys.ValidTekCountSecondPass, count);
        }

        public static void WriteTekDuplicatesFoundWholeWorkflow(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Tek duplicates found - Whole Workflow - Messages:{messages}", LoggingDataPostKeys.Name, LoggingDataPostKeys.TekDuplicatesFoundWholeWorkflow, string.Join(",", messages));
        }
        public static void WriteDbWriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Writing db.", LoggingDataPostKeys.Name, LoggingDataPostKeys.DbWriteStart);
        }

        public static void WriteDbWriteCommitted(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Committed.", LoggingDataPostKeys.Name, LoggingDataPostKeys.DbWriteCommitted);
        }
        public static void WriteTekCountAdded(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Count:{count}.", LoggingDataPostKeys.Name, LoggingDataPostKeys.TekCountAdded, count);
        }
    }
}

