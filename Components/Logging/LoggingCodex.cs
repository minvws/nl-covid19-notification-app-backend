// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging
{
    internal static class LoggingCodex
    {
        //Current ordering is by project; keeping the initial numbering in place.
        //Under MobileAppApi
        public const int Register = 100;
        public const int PostTeks = 200;
        public const int Decoy = 300;
        public const int ResponsePaddding = 400;
        public const int ExceptionInterceptor = 500;
        public const int SuppressError = 600;
        public const int GetCdnContent = 700;
        // 800 is for IccBackend; to be implemented later
        public const int DbProvision = 900;
        public const int PublishContent = 1000;
        public const int SigtestFileCreator = 1100;
        public const int DailyCleanup = 1200;
        public const int RemoveExpiredManifest = 1300;
        public const int RemoveExpiredEks = 1400;
    }

    public static class LoggingDataRegisterSecret
    {
        public const string Name = "Register";

        public const int Start = LoggingCodex.Register;
        public const int Finished = LoggingCodex.Register + 99;

        public const int Writing = LoggingCodex.Register + 1;
        public const int Committed = LoggingCodex.Register + 2;
        public const int DuplicatesFound = LoggingCodex.Register + 3;

        public const int MaximumCreateAttemptsReached = LoggingCodex.Register + 4;
        public const int Failed = LoggingCodex.Register + 5;
    }

    public static class LoggingDataPostKeys
    {
        public const string Name = "Postkeys";
        public const int Start = LoggingCodex.PostTeks;
        public const int Finished = LoggingCodex.PostTeks + 99;

        public const int SignatureValidationFailed = LoggingCodex.PostTeks + 1;
        public const int BucketIdParsingFailed = LoggingCodex.PostTeks + 2;
        public const int TekValidationFailed = LoggingCodex.PostTeks + 3;
        public const int ValidTekCount = LoggingCodex.PostTeks + 4;
        public const int BucketDoesNotExist = LoggingCodex.PostTeks + 5;
        public const int SignatureInvalid = LoggingCodex.PostTeks + 6;
        public const int WorkflowFilterResults = LoggingCodex.PostTeks + 7;
        public const int ValidTekCountSecondPass = LoggingCodex.PostTeks + 8;
        public const int TekDuplicatesFoundWholeWorkflow = LoggingCodex.PostTeks + 9;
        public const int DbWriteStart = LoggingCodex.PostTeks + 10;
        public const int DbWriteCommitted = LoggingCodex.PostTeks + 11;
        public const int TekCountAdded = LoggingCodex.PostTeks + 12;
    }

    public static class LoggingDataDecoy
    {
        public const string Name = "Decoykeys(PostSecret)";

        public const int Start = LoggingCodex.Decoy;
    }

    public static class LoggingDataResponsePadding
	{
        public const string Name = "PostDecoyPadding";

        public const int NoPaddingNeeded = LoggingCodex.ResponsePaddding + 1;
        public const int ResponsePaddingLength = LoggingCodex.ResponsePaddding + 2;
        public const int ResponsePaddingContent = LoggingCodex.ResponsePaddding + 3;
        public const int PaddingAdded = LoggingCodex.ResponsePaddding + 4;
    }

    public static class LoggingDataExceptioninterceptor
	{
        public const string Name = "MsLoggerServiceExceptionInterceptor";

        public const int ExceptionFound = LoggingCodex.ExceptionInterceptor;
    }

    public static class LoggingDataSuppressError
	{
        public const string Name = "SuppressError";

        public const int CallFailed = LoggingCodex.SuppressError;
    }

    public static class LoggingDataGetCdnContent
	{
        public const string Name = "HttpGetCdnContent";

        public const int InvalidType = LoggingCodex.GetCdnContent;
        public const int InvalidId = LoggingCodex.GetCdnContent + 1;
        public const int HeaderMissing = LoggingCodex.GetCdnContent + 2;
        public const int NotFound = LoggingCodex.GetCdnContent + 3;
        public const int EtagFound = LoggingCodex.GetCdnContent + 4;
    }

    public static class LoggingDataDbProvision
	{
        public const string Name = "DbProvision";

        public const int Start = LoggingCodex.DbProvision;
        public const int Finished = LoggingCodex.DbProvision + 99;

        public const int WorkflowDb = LoggingCodex.DbProvision + 1;
        public const int ContentDb = LoggingCodex.DbProvision + 2;
        public const int JobDb = LoggingCodex.DbProvision + 3;
    }

    public static class LoggingDataPublishContent
	{
        public const string Name = "PublishContent";

        public const int StartWriting = LoggingCodex.PublishContent;
        public const int FinishedWriting = LoggingCodex.PublishContent + 1;
	}

    public static class LoggingDataSigTestFileCreator
	{
        public const string Name = "SigTestFileCreator";

        public const int Start = LoggingCodex.SigtestFileCreator;
        public const int Finished = LoggingCodex.SigtestFileCreator + 99;

        public const int NoElevatedPrivs = LoggingCodex.SigtestFileCreator + 1;
        public const int BuildingResultFile = LoggingCodex.SigtestFileCreator + 2;
        public const int SavingResultFile = LoggingCodex.SigtestFileCreator + 3;
	}

    public static class LoggingDataDailyCleanup
	{
        public const string Name = "DailyCleanup";

        public const int Start = LoggingCodex.DailyCleanup;
        public const int Finished = LoggingCodex.DailyCleanup + 99;

        public const int EksEngineStarting = LoggingCodex.DailyCleanup + 1;
        public const int ManifestEngineStarting = LoggingCodex.DailyCleanup + 2;
        public const int DailyStatsCalcStarting = LoggingCodex.DailyCleanup + 3;
        public const int ManiFestCleanupStarting = LoggingCodex.DailyCleanup + 4;
        public const int EksCleanupStarting = LoggingCodex.DailyCleanup + 5;
        public const int WorkflowCleanupStarting = LoggingCodex.DailyCleanup + 6;
        public const int ResignerStarting = LoggingCodex.DailyCleanup + 7;
        public const int EksV2CleanupStarting = LoggingCodex.DailyCleanup + 8;
        public const int ManifestV2CleanupStarting = LoggingCodex.DailyCleanup + 9;       
    }

    public static class LoggingDataRemoveExpiredManifest
	{
        public const string Name = "RemoveExpiredManifest";

        public const int Start = LoggingCodex.RemoveExpiredManifest;
        public const int Finished = LoggingCodex.RemoveExpiredManifest + 99;

        public const int RemovingManifests = LoggingCodex.RemoveExpiredManifest + 1;
        public const int RemovingEntry = LoggingCodex.RemoveExpiredManifest + 2;
        public const int ReconciliationFailed = LoggingCodex.RemoveExpiredManifest + 3;
        public const int DeletionReconciliationFailed = LoggingCodex.RemoveExpiredManifest + 4;
        public const int FinishedNothingRemoved = LoggingCodex.RemoveExpiredManifest + 98;
	}

    public static class LoggingDataRemoveExpiredEks
	{
        const string Name = "RemoveExpiredEks";

        public const int Start = LoggingCodex.RemoveExpiredEks;
        public const int Finished = LoggingCodex.RemoveExpiredEks + 99;

        public const int CurrentEksFound = LoggingCodex.RemoveExpiredEks + 1;
        public const int FoundTotal = LoggingCodex.RemoveExpiredEks + 2;
        public const int FoundEntry = LoggingCodex.RemoveExpiredEks + 3;
        public const int RemovedAmount = LoggingCodex.RemoveExpiredEks + 4;
        public const int ReconciliationFailed = LoggingCodex.RemoveExpiredEks + 5;
        public const int FinishedNothingRemoved = LoggingCodex.RemoveExpiredEks + 98;
	}
}
