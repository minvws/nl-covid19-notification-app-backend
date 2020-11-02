using System;
using Microsoft.Extensions.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging.PostKeys
{
    public static class LoggingExtensionsPostKeys
    {
        private const string Name = "Postkeys";
        private const int Base = LoggingCodex.PostTeks;

        private const int Start = Base;
        private const int Finished = Base + 99;
        private const int SignatureValidationFailed = Base + 1;
        private const int BucketIdParsingFailed = Base + 2;
        private const int TekValidationFailed = Base + 3;
        private const int ValidTekCount = Base + 4;
        private const int BucketDoesNotExist = Base + 5;
        private const int SignatureInvalid = Base + 6;
        private const int WorkflowFilterResults = Base + 7;
        private const int ValidTekCountSecondPass = Base + 8;
        private const int TekDuplicatesFoundWholeWorkflow = Base + 9;
        private const int DbWriteStart = Base + 10;
        private const int DbWriteCommitted = Base + 11;
        private const int TekCountAdded = Base + 12;

        public static void WriteStartPostKeys(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] POST triggered.", Name, Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Finished.", Name, Finished);
        }

        public static void WriteSignatureValidationFailed(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Signature is null or incorrect length.", Name, SignatureValidationFailed);
        }

        public static void WritePostBodyParsingFailed(this ILogger logger, Exception e)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation(e, "[{name}/{id}] Error reading body");
        }

        public static void WriteBucketIdParsingFailed(this ILogger logger, string input, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] BucketId failed validation - BucketId:{input} Messages:{messages}", Name, BucketIdParsingFailed, input, string.Join(",", messages));
        }

        public static void WriteTekValidationFailed(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:{messages}", Name, TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteTekDuplicatesFound(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Tek duplicates found - Messages:{messages}", Name, TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteApplicableWindowFilterResult(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:{messages}", Name, TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteValidTekCount(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] TEKs remaining - Count:{Count}.", Name, ValidTekCount, count);
        }

        public static void WriteBucketDoesNotExist(this ILogger logger, string bucketId)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogError("[{name}/{id}] Bucket does not exist - Id:{BucketId}.", Name, BucketDoesNotExist, bucketId); //_ArgsObject.BucketId
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
            logger.LogError("[{name}/{id}] Signature not valid - Signature:{Signature} Bucket:{}", Name, SignatureInvalid, Convert.ToBase64String(signature), Convert.ToBase64String(bucketId));
        }

        public static void WriteWorkflowFilterResults(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] WriteWorkflowFilterResults - Count:{Count}.", Name, WorkflowFilterResults, string.Join(",", messages));
        }

        public static void WriteValidTekCountSecondPass(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] TEKs remaining 2 - Count:{Count}.", Name, ValidTekCountSecondPass, count);
        }

        public static void WriteTekDuplicatesFoundWholeWorkflow(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Tek duplicates found - Whole Workflow - Messages:{messages}", Name, TekDuplicatesFoundWholeWorkflow, string.Join(",", messages));
        }
        public static void WriteDbWriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Writing db.", Name, DbWriteStart);
        }

        public static void WriteDbWriteCommitted(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Committed.", Name, DbWriteCommitted);
        }
        public static void WriteTekCountAdded(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Count:{count}.", Name, TekCountAdded, count);
        }
    }
}

