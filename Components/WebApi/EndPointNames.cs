// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    public static class EndPointNames
    {
        public static class MobileAppApi
        {
                private const string Prefix = "/v1";
                public const string RegisterSecret = Prefix + "/register";
                public const string ReleaseTeks = Prefix + "/postkeys";
                public const string RandomNoise = Prefix + "/stopkeys";
        }

        /// <summary>
        /// Use the same ones for CaregiversPortalDataApi
        /// </summary>
        public static class CaregiversPortalApi
        {
            private const string Prefix = "/CaregiversPortalApi/v1";
            public const string LabConfirmation = Prefix + "/labconfirm";
        }

        public static class DevOps
        {
            private const string DevOpsPrefix = "/devops";

            public const string NukeAndPave = DevOpsPrefix + "/nukeandpavedb"; 
            public const string NukeAndPaveIcc = DevOpsPrefix + "/nukeandpavedb/icc"; 

            public const string ExposureKeySetsCreate = DevOpsPrefix + "/exposurekeysets/runbatchjob";


            public const string TekSetsGenerateRandom = DevOpsPrefix + "/teks/random";
            public const string TekSetsAuthorise = DevOpsPrefix + "/teks/authorise";
        }

        public static class ContentAdminPortalDataApi
        {
            private const string Prefix = "/ContentAdmin/v1";

            public const string AppConfig = Prefix + "/appconfig";
            public const string ResourceBundle = Prefix + "/resourcebundle";
            public const string ExposureKeySet = Prefix + "/exposurekeyset";
            public const string RiskCalculationParameters = Prefix + "/riskcalculationparameters";
        }

        public const string ManifestName = "manifest";

        //Also used for the data api where necessary
        public static class CdnApi
        {
            private const string Prefix = "/v1";
            public const string Manifest = Prefix + "/manifest";
            public const string AppConfig = Prefix + "/appconfig";
            public const string ResourceBundle = Prefix + "/resourcebundle";
            public const string ExposureKeySet = Prefix + "/exposurekeyset";
            public const string RiskCalculationParameters = Prefix + "/riskcalculationparameters";
        }
    }
}
