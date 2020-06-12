namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.WebApi
{
    public static class EndPointNames
    {
        public static class MobileAppApi
        {
            public static class KeysFirstWorkflow
            {
                private const string Prefix = "/keysfirst/v1";
                public const string Teks = Prefix + "/teks";
            }

            public static class KeysLastWorkflow
            {
                private const string Prefix = "/keyslast/v1";
                public const string RegisterSecret = Prefix + "/register";
                public const string ReleaseTeks = Prefix + "/postkeys";
                public const string RandomNoise = Prefix + "/stopkeys";
            }
        }


        /// <summary>
        /// Use the same ones for CaregiversPortalDataApi
        /// </summary>
        public static class CaregiversPortalApi
        {
            public static class KeysFirstWorkflow
            {
                private const string Prefix = "/keysfirst";
                public const string Authorise = Prefix+"/authorise";
            }

            public static class KeysLastWorkflow
            {
                private const string Prefix = "/keyslast";
                public const string LabConfirmation = Prefix + "/labconfirm";

                //Workflow tba.
            }
        }

        public static class DevOps
        {
            private const string DevOpsPrefix = "/devops";

            public const string NukeAndPave = DevOpsPrefix + "/nukeandpavedb";

            public const string ExposureKeySetsCreate = DevOpsPrefix + "/exposurekeysets/runbatchjob";

            public class KeysFirstWorkFlow
            {
                private const string Prefix = DevOpsPrefix + "/keysfirst";

                public const string TekSetsGenerateRandom = Prefix + "/teks/random";
                public const string TekSetsRandomAuthorisation = Prefix + "/teks/authorise";
            }

            public class KeysLastWorkFlow
            {
                private const string Prefix = DevOpsPrefix + "/keyslast";
                public const string TekSetsGenerateRandom = Prefix + "/teks/random";
                public const string TekSetsAuthorise = Prefix + "/teks/authorise";
            }
        }

        public static class ContentAdminPortalDataApi
        {
            private const string Prefix = "/contentadmin/v1";

            public const string ResourceBundle = Prefix + "/resourcebundle";
            public const string ExposureKeySet = Prefix + "/exposurekeyset";
            public const string RiskCalculationParameters = Prefix + "/riskcalculationparameters";
        }

        //Also used for the data api where necessary
        public static class CdnApi
        {
            private const string Prefix = "/cdn/v1";
            public const string Manifest = Prefix + "/manifest";
            public const string AppConfig = Prefix + "/appconfig";
            public const string ResourceBundle = Prefix + "/resourcebundle";
            public const string ExposureKeySet = Prefix + "/exposurekeyset";
            public const string RiskCalculationParameters = Prefix + "/riskcalculationparameters";
        }
    }
}
