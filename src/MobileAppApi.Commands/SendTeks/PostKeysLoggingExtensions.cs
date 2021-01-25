// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands.SendTeks
{
    public class PostKeysLoggingExtensions
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

        private readonly ILogger _Logger;

        public PostKeysLoggingExtensions(ILogger<PostKeysLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStartPostKeys()
        {
            _Logger.LogInformation("[{name}/{id}] POST triggered.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _Logger.LogDebug("[{name}/{id}] Finished.",
                Name, Finished);
        }

        public void WriteSignatureValidationFailed()
        {
            _Logger.LogInformation("[{name}/{id}] Signature is null or incorrect length.",
                Name, SignatureValidationFailed);
        }

        public void WritePostBodyParsingFailed(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _Logger.LogInformation(exception, "[{name}/{id}] Error reading body");
        }

        public void WriteBucketIdParsingFailed(string input, string[] messages)
        {
            _Logger.LogInformation("[{name}/{id}] BucketId failed validation - BucketId:{input} Messages:\n{messages}",
                Name, BucketIdParsingFailed,
                input, string.Join("\n", messages));
        }

        public void WriteTekValidationFailed(string[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }

            _Logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:\n{messages}",
                Name, TekValidationFailed,
                string.Join("\n", messages));
        }

        public void WriteTekDuplicatesFound(string[] messages)
        {
            _Logger.LogInformation("[{name}/{id}] Tek duplicates found - Messages:\n{messages}",
                Name, TekValidationFailed,
                string.Join("\n", messages));
        }

        public void WriteApplicableWindowFilterResult(string[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }

            _Logger.LogInformation("[{name}/{id}] Tek failed validation - Messages:\n{messages}",
                Name, TekValidationFailed,
                string.Join("\n", messages));
        }

        public void WriteValidTekCount(int count)
        {
            _Logger.LogInformation("[{name}/{id}] TEKs remaining - Count:{Count}.",
                Name, ValidTekCount,
                count);
        }

        public void WriteBucketDoesNotExist(string bucketId)
        {
            _Logger.LogError("[{name}/{id}] Bucket does not exist - Id:{BucketId}.",
                Name, BucketDoesNotExist,
                bucketId);
        }

        public void WriteSignatureInvalid(byte[] bucketId, byte[] signature)
        {
            _Logger.LogError("[{name}/{id}] Signature not valid - Signature:{Signature} Bucket:{BucketId}",
                Name, SignatureInvalid,
                Convert.ToBase64String(signature), Convert.ToBase64String(bucketId));
        }

        public void WriteWorkflowFilterResults(string[] messages)
        {
            if (messages.Length == 0)
            {
                return;
            }

            _Logger.LogInformation("[{name}/{id}] WriteWorkflowFilterResults:\n{Messages}.",
                Name, WorkflowFilterResults,
                string.Join("\n", messages));
        }

        public void WriteValidTekCountSecondPass(int count)
        {
            _Logger.LogInformation("[{name}/{id}] TEKs remaining 2 - Count:{Count}.",
                Name, ValidTekCountSecondPass,
                count);
        }

        public void WriteTekDuplicatesFoundWholeWorkflow(string[] messages)
        {
            _Logger.LogInformation("[{name}/{id}] Tek duplicates found - Whole Workflow - Messages:\n{messages}",
                Name, TekDuplicatesFoundWholeWorkflow,
                string.Join("\n", messages));
        }
        public void WriteDbWriteStart()
        {
            _Logger.LogInformation("[{name}/{id}] Teks added - Writing db.",
                Name, DbWriteStart);
        }

        public void WriteDbWriteCommitted()
        {
            _Logger.LogInformation("[{name}/{id}] Teks added - Committed.",
                Name, DbWriteCommitted);
        }
        public void WriteTekCountAdded(int count)
        {
            _Logger.LogInformation("[{name}/{id}] Teks added - Count:{count}.",
                Name, TekCountAdded,
                count);
        }
    }
}

