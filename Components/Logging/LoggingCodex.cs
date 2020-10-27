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
        public const int RemoveExpiredWorkflow = 1500;
        public const int Resigner = 1600;
        public const int RemoveExpiredEksV2 = 1700;
        public const int RemoveExpiredManifestV2 = 1800;
        public const int EksEngine = 1900;
        public const int Snapshot = 2000;
        public const int EksBuilderV1 = 2100;
        public const int EksJobContentWriter = 2200;
        public const int MarkWorkflowTeksAsUsed = 2300;
        public const int ManifestUpdate = 2400;
        public const int EmbeddedCertProvider = 2500;
        public const int CertLmProvider = 2600;
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
        public const string Name = "RemoveExpiredEks";

        public const int Start = LoggingCodex.RemoveExpiredEks;
        public const int Finished = LoggingCodex.RemoveExpiredEks + 99;

        public const int CurrentEksFound = LoggingCodex.RemoveExpiredEks + 1;
        public const int FoundTotal = LoggingCodex.RemoveExpiredEks + 2;
        public const int FoundEntry = LoggingCodex.RemoveExpiredEks + 3;
        public const int RemovedAmount = LoggingCodex.RemoveExpiredEks + 4;
        public const int ReconciliationFailed = LoggingCodex.RemoveExpiredEks + 5;
        public const int FinishedNothingRemoved = LoggingCodex.RemoveExpiredEks + 98;
	}

    public static class LoggingDataRemoveExpiredWorkflow
	{
        public const string Name = "RemoveExpiredWorkflow";

        public const int Start = LoggingCodex.RemoveExpiredWorkflow;
        public const int Finished = LoggingCodex.RemoveExpiredWorkflow + 99;
        
        public const int Report = LoggingCodex.RemoveExpiredWorkflow + 1;
        public const int RemovedAmount = LoggingCodex.RemoveExpiredWorkflow + 2;
        public const int UnpublishedTekFound = LoggingCodex.RemoveExpiredWorkflow + 97;
        public const int FinishedNothingRemoved = LoggingCodex.RemoveExpiredWorkflow + 98;
    }

    public static class LoggingDataResigner
	{
        public const string Name = "Resigner";

        public const int Finished = LoggingCodex.Resigner + 99;

        public const int CertNotSpecified = LoggingCodex.Resigner + 1;
        public const int Report = LoggingCodex.Resigner + 2;
	}

    public static class LoggingDataRemoveExpiredEksV2
	{
        public const string Name = "RemoveExpiredEksV2";

        public const int Start = LoggingCodex.RemoveExpiredEksV2;
        public const int Finished = LoggingCodex.RemoveExpiredEksV2 + 99;

        public const int CurrentEksFound = LoggingCodex.RemoveExpiredEksV2 + 1;
        public const int FoundTotal = LoggingCodex.RemoveExpiredEksV2 + 2;
        public const int FoundEntry = LoggingCodex.RemoveExpiredEksV2 + 3;
        public const int RemovedAmount = LoggingCodex.RemoveExpiredEksV2 + 4;
        public const int ReconciliationFailed = LoggingCodex.RemoveExpiredEksV2 + 5;
        public const int FinishedNothingRemoved = LoggingCodex.RemoveExpiredEksV2 + 98;

    }

    public static class LoggingDataRemoveExpiredManifestV2
	{
        public const string Name = "RemoveExpiredManifestV2";

        public const int Start = LoggingCodex.RemoveExpiredManifestV2;
        public const int Finished = LoggingCodex.RemoveExpiredManifestV2 + 99;

        public const int RemovingManifests = LoggingCodex.RemoveExpiredManifestV2 + 1;
        public const int RemovingEntry = LoggingCodex.RemoveExpiredManifestV2 + 2;
        public const int ReconciliationFailed = LoggingCodex.RemoveExpiredManifestV2 + 3;
        public const int DeletionReconciliationFailed = LoggingCodex.RemoveExpiredManifestV2 + 4;
        public const int FinishedNothingRemoved = LoggingCodex.RemoveExpiredManifestV2 + 98;
    }

    public static class LoggingDataEksEngine
	{
        public const string Name = "EksEngine";

        public const int Start = LoggingCodex.EksEngine;
        public const int Finished = LoggingCodex.EksEngine + 99;

        public const int NoElevatedPrivs = LoggingCodex.EksEngine + 1;
        public const int ReconciliationTeksMatchInputAndStuffing = LoggingCodex.EksEngine + 2;
        public const int ReconcilliationTeksMatchOutputCount = LoggingCodex.EksEngine + 3;
        
        //ClearJobTables()
        public const int ClearJobTables = LoggingCodex.EksEngine + 4;
        
        //Stuff()
        public const int NoStuffingNoTeks = LoggingCodex.EksEngine + 5;
        public const int NoStuffingMinimumOk = LoggingCodex.EksEngine + 6;
        public const int StuffingRequired = LoggingCodex.EksEngine + 7;
        public const int StuffingAdded = LoggingCodex.EksEngine + 8;
        
        //BuildOutput()
        public const int BuildEkses = LoggingCodex.EksEngine + 9;
        public const int ReadTeks = LoggingCodex.EksEngine + 10;
        public const int PageFillsToCapacity = LoggingCodex.EksEngine + 11;
        public const int WriteRemainingTeks = LoggingCodex.EksEngine + 12;
        
        //AddToOutput()
        public const int AddTeksToOutput = LoggingCodex.EksEngine + 13;

        //WriteNewEksToOutput()
        public const int BuildEntry = LoggingCodex.EksEngine + 14;
        public const int WritingCurrentEks = LoggingCodex.EksEngine + 15;
        public const int MarkTekAsUsed = LoggingCodex.EksEngine + 16;
        
        //GetInputPage()
        public const int StartReadPage = LoggingCodex.EksEngine + 17;
        public const int FinishReadPage = LoggingCodex.EksEngine + 18;
        
        //CommitResults()
        public const int CommitPublish = LoggingCodex.EksEngine + 19;
        public const int CommitMarkTeks = LoggingCodex.EksEngine + 20;
        public const int TotalMarked = LoggingCodex.EksEngine + 21;   
	}

    public static class LoggingDataSnapshot
	{
        public const string Name = "Snapshot";

        public const int Start = LoggingCodex.Snapshot;

        public const int TeksToPublish = LoggingCodex.Snapshot + 1;
	}

    public static class LoggingDataEksBuilderV1
	{
        public const string Name = "EksBuilderV1";

        public const int NlSig = LoggingCodex.EksBuilderV1 + 1;
        public const int GaenSig = LoggingCodex.EksBuilderV1 + 2;
	}

    public static class LoggingDataMarkWorkFlowTeksAsUsed
	{
        public const string Name = "MarkWorkFlowTeksAsUsed";

        public const int MarkAsPublished = LoggingCodex.MarkWorkflowTeksAsUsed + 1;
	}

    public static class LoggingDataEksJobContentWriter
	{
        public const string Name = "EksJobContentWriter";

        public const int Published = LoggingCodex.EksJobContentWriter + 1;
	}

    public static class LoggingDataManifestUpdateCommand
	{
        public static string Name = "ManifestUpdateCommand";

        public static int Start = LoggingCodex.ManifestUpdate;
        public static int Finished = LoggingCodex.ManifestUpdate + 99;

        public static int UpdateNotRequired = LoggingCodex.ManifestUpdate + 1;
	}

    public static class LoggingDataEmbeddedResourceCertificateProvider
	{
        public static string Name = "EmbeddedResourceCertificateProvider";

        public static int Opening = LoggingCodex.EmbeddedCertProvider + 1;
        public static int ResourceFound = LoggingCodex.EmbeddedCertProvider + 2;
        public static int ResourceFail = LoggingCodex.EmbeddedCertProvider + 3;
    }

    public static class LoggingDataCertLmProvider
	{
        public static string Name = "LocalMachineStoreCertificateProvider";

        public static int Finding = LoggingCodex.CertLmProvider + 1;
        public static int CertNotFound = LoggingCodex.CertLmProvider + 2;
        public static int NoPrivateKey = LoggingCodex.CertLmProvider + 3;
        public static int CertReadError = LoggingCodex.CertLmProvider + 4;

    }
}
