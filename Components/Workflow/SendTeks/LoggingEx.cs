using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.SendTeks
{
    public static class LoggingEx
    {
        private const string Name = "postkeys";

        private static class Ids
        {
            public const int Start = LoggingIdScopeStart.PostTeks;
            public const int Finished = LoggingIdScopeStart.PostTeks + 99;

            public const int SignatureValidationFailed = LoggingIdScopeStart.PostTeks + 1;
            public const int BucketIdParsingFailed = LoggingIdScopeStart.PostTeks + 2;
            public const int TekValidationFailed = LoggingIdScopeStart.PostTeks + 3;
            public const int ValidTekCount = LoggingIdScopeStart.PostTeks + 4;
            public const int BucketDoesNotExist = LoggingIdScopeStart.PostTeks + 5;
            public const int SignatureInvalid = LoggingIdScopeStart.PostTeks + 6;
            public const int WorkflowFilterResults = LoggingIdScopeStart.PostTeks + 7;
            public const int ValidTekCountSecondPass = LoggingIdScopeStart.PostTeks + 8;
            public const int TekDuplicatesFoundWholeWorkflow = LoggingIdScopeStart.PostTeks + 9;
            public const int DbWriteStart = LoggingIdScopeStart.PostTeks + 10;
            public const int DbWriteCommitted = LoggingIdScopeStart.PostTeks + 11;
            public const int TekCountAdded = LoggingIdScopeStart.PostTeks + 12;
        }

        public static void WriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] POST triggered.", Name, Ids.Start);
        }

        public static void WriteFinished(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogDebug("[{name}/{id}] Finished.", Name, Ids.Finished);
        }

        public static void WriteSignatureValidationFailed(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Signature is null or incorrect length.", Name, Ids.SignatureValidationFailed);
        }

        public static void WritePostBodyParsingFailed(this ILogger logger, Exception e)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation(e, "[{name}/{id}] Error reading body");
        }

        public static void WriteBucketIdParsingFailed(this ILogger logger, string input, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] BucketId failed validation - BucketId:{input} Messages:{messages}", Name, Ids.BucketIdParsingFailed, input, string.Join(",", messages));
        }

        public static void WriteTekValidationFailed(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:{messages}", Name, Ids.TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteTekDuplicatesFound(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Tek duplicates found - Messages:{messages}", Name, Ids.TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteApplicableWindowFilterResult(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:{messages}", Name, Ids.TekValidationFailed, string.Join(",", messages));
        }

        public static void WriteValidTekCount(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] TEKs remaining - Count:{Count}.", Name, Ids.ValidTekCount, count);
        }

        public static void WriteBucketDoesNotExist(this ILogger logger, string bucketId)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogError("[{name}/{id}] Bucket does not exist - Id:{BucketId}.", Name, Ids.BucketDoesNotExist, bucketId); //_ArgsObject.BucketId
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
            logger.LogError("[{name}/{id}] Signature not valid - Signature:{Signature} Bucket:{}", Name, Ids.SignatureInvalid, Convert.ToBase64String(signature), Convert.ToBase64String(bucketId));
        }

        public static void WriteWorkflowFilterResults(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (messages.Length == 0) return;
            logger.LogInformation("[{name}/{id}] WriteWorkflowFilterResults - Count:{Count}.", Name, Ids.WorkflowFilterResults, string.Join(",", messages));
        }

        public static void WriteValidTekCountSecondPass(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] TEKs remaining 2 - Count:{Count}.", Name, Ids.ValidTekCountSecondPass, count);
        }

        public static void WriteTekDuplicatesFoundWholeWorkflow(this ILogger logger, string[] messages)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Tek duplicates found - Whole Workflow - Messages:{messages}", Name, Ids.TekDuplicatesFoundWholeWorkflow, string.Join(",", messages));
        }
        public static void WriteDbWriteStart(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Writing db.", Name, Ids.DbWriteStart);
        }

        public static void WriteDbWriteCommitted(this ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Committed.", Name, Ids.DbWriteCommitted);
        }
        public static void WriteTekCountAdded(this ILogger logger, int count)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            logger.LogInformation("[{name}/{id}] Teks added - Count:{count}.", Name, Ids.TekCountAdded, count);
        }
    }
}

