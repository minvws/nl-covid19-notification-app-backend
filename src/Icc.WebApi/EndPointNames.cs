// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

namespace NL.Rijksoverheid.ExposureNotification.Icc.WebApi
{
    public static class EndPointNames
    {
        public static class MobileAppApi
        {
                private const string Prefix = "/v1";
                public const string Register = Prefix + "/register";
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


        public const string ManifestName = "manifest";
        public const string AppConfigName = "appconfig";
        public const string ExposureKeySetName = "exposurekeyset";
        public const string RiskCalculationParametersName = "riskcalculationparameters";

        //Also used for the data api where necessary
        public static class ContentApi
        {
            private const string Prefix = "/v1";
            public const string Manifest = Prefix + "/" + ManifestName;
            public const string AppConfig = Prefix + "/" + AppConfigName; //TODO WHY IS THIS NO LONGER IN USE?
            public const string ExposureKeySet = Prefix + "/" + ExposureKeySetName;
            public const string RiskCalculationParameters = Prefix + "/" + RiskCalculationParametersName; //TODO WHY IS THIS NO LONGER IN USE?
        }
    }
}
