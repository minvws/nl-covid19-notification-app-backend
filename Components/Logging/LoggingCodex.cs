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
}
