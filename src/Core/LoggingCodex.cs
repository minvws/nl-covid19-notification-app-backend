// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Core
{
    public static class LoggingCodex
    {
        public const int Register = 1100;
        public const int PostTeks = 1200;
        public const int Decoy = 1300;
        public const int ResponsePadding = 1400;
        public const int ExceptionInterceptor = 1500;
        public const int SuppressError = 1600;
        public const int GetCdnContent = 1700;
        public const int IccBackend = 1800;
        public const int DbProvision = 1900;
        public const int PublishContent = 2000;
        public const int SigtestFileCreator = 2100;
        public const int DailyCleanup = 2200;
        public const int RemoveExpiredManifest = 2300;
        public const int RemoveExpiredEks = 2400;
        public const int RemoveExpiredWorkflow = 2500;
        public const int Resigner = 2600;
        public const int RemoveExpiredEksV2 = 2700;
        public const int RemoveExpiredManifestV2 = 2800;
        public const int EksEngine = 2900;
        public const int Snapshot = 3000;
        public const int EksBuilderV1 = 3100;
        public const int EksJobContentWriter = 3200;
        public const int MarkWorkflowTeksAsUsed = 3300;
        public const int ManifestUpdate = 3400;
        public const int EmbeddedCertProvider = 3500;
        public const int CertLmProvider = 3600;
        public const int IksDownloader = 3700;
        public const int IksUploader = 3800;
        public const int IKSEngine = 3900;
        public const int WorkFlowTeksToDksCommand = 4000;
        public const int IksImportBatchJob = 4100;
        public const int ConsoleAppRunner = 4200; //not entirely sure yet
        public const int RemoveExpiredIks = 4300; //not entirely sure yet
        public const int RemoveExpiredManifestV3 = 4400;
        public const int RemoveExpiredManifestV4 = 4500;
        public const int RegisterV2 = 4600;
    }
}
