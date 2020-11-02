// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Logging
{
    public static class LoggingCodex
    {
        //Current ordering is by project; keeping the initial numbering in place.
        //Under MobileAppApi
        public const int Register = 100;
        public const int PostTeks = 200;
        public const int Decoy = 300;
        public const int ResponsePadding = 400;
        public const int ExceptionInterceptor = 500;
        public const int SuppressError = 600;
        public const int GetCdnContent = 700;
        public const int IccBackend = 800;
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
}
