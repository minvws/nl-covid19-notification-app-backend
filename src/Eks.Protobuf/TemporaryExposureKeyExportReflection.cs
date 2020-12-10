// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf.Reflection;
using NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Protobuf
{
    /// <summary>Holder for reflection information generated from TemporaryExposureKeyExport.proto</summary>
    public static partial class TemporaryExposureKeyExportReflection {

        #region Descriptor
        /// <summary>File descriptor for TemporaryExposureKeyExport.proto</summary>
        public static FileDescriptor Descriptor {
            get { return descriptor; }
        }
        private static FileDescriptor descriptor;

        static TemporaryExposureKeyExportReflection() {
            var descriptorData = System.Convert.FromBase64String(
                string.Concat(
                    "CiBUZW1wb3JhcnlFeHBvc3VyZUtleUV4cG9ydC5wcm90byL+AQoaVGVtcG9y",
                    "YXJ5RXhwb3N1cmVLZXlFeHBvcnQSFwoPc3RhcnRfdGltZXN0YW1wGAEgASgG",
                    "EhUKDWVuZF90aW1lc3RhbXAYAiABKAYSDgoGcmVnaW9uGAMgASgJEhEKCWJh",
                    "dGNoX251bRgEIAEoBRISCgpiYXRjaF9zaXplGAUgASgFEicKD3NpZ25hdHVy",
                    "ZV9pbmZvcxgGIAMoCzIOLlNpZ25hdHVyZUluZm8SIwoEa2V5cxgHIAMoCzIV",
                    "LlRlbXBvcmFyeUV4cG9zdXJlS2V5EisKDHJldmlzZWRfa2V5cxgIIAMoCzIV",
                    "LlRlbXBvcmFyeUV4cG9zdXJlS2V5IpsBCg1TaWduYXR1cmVJbmZvEhUKDWFw",
                    "cF9idW5kbGVfaWQYASABKAkSFwoPYW5kcm9pZF9wYWNrYWdlGAIgASgJEiAK",
                    "GHZlcmlmaWNhdGlvbl9rZXlfdmVyc2lvbhgDIAEoCRIbChN2ZXJpZmljYXRp",
                    "b25fa2V5X2lkGAQgASgJEhsKE3NpZ25hdHVyZV9hbGdvcml0aG0YBSABKAki",
                    "7AIKFFRlbXBvcmFyeUV4cG9zdXJlS2V5EhAKCGtleV9kYXRhGAEgASgMEiMK",
                    "F3RyYW5zbWlzc2lvbl9yaXNrX2xldmVsGAIgASgFQgIYARIlCh1yb2xsaW5n",
                    "X3N0YXJ0X2ludGVydmFsX251bWJlchgDIAEoBRIbCg5yb2xsaW5nX3Blcmlv",
                    "ZBgEIAEoBToDMTQ0EjUKC3JlcG9ydF90eXBlGAUgASgOMiAuVGVtcG9yYXJ5",
                    "RXhwb3N1cmVLZXkuUmVwb3J0VHlwZRIkChxkYXlzX3NpbmNlX29uc2V0X29m",
                    "X3N5bXB0b21zGAYgASgRInwKClJlcG9ydFR5cGUSCwoHVU5LTk9XThAAEhIK",
                    "DkNPTkZJUk1FRF9URVNUEAESIAocQ09ORklSTUVEX0NMSU5JQ0FMX0RJQUdO",
                    "T1NJUxACEg8KC1NFTEZfUkVQT1JUEAMSDQoJUkVDVVJTSVZFEAQSCwoHUkVW",
                    "T0tFRBAFIjUKEFRFS1NpZ25hdHVyZUxpc3QSIQoKc2lnbmF0dXJlcxgBIAMo",
                    "CzINLlRFS1NpZ25hdHVyZSJwCgxURUtTaWduYXR1cmUSJgoOc2lnbmF0dXJl",
                    "X2luZm8YASABKAsyDi5TaWduYXR1cmVJbmZvEhEKCWJhdGNoX251bRgCIAEo",
                    "BRISCgpiYXRjaF9zaXplGAMgASgFEhEKCXNpZ25hdHVyZRgEIAEoDEJEqgJB",
                    "TkwuUmlqa3NvdmVyaGVpZC5FeHBvc3VyZU5vdGlmaWNhdGlvbi5CYWNrRW5k",
                    "LkdlbmVyYXRlZEdhZW5Gb3JtYXQ="));
            descriptor = FileDescriptor.FromGeneratedCode(descriptorData,
                new FileDescriptor[] { },
                new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[] {
                    new GeneratedClrTypeInfo(typeof(TemporaryExposureKeyExport), TemporaryExposureKeyExport.Parser, new[]{ "StartTimestamp", "EndTimestamp", "Region", "BatchNum", "BatchSize", "SignatureInfos", "Keys", "RevisedKeys" }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(SignatureInfo), SignatureInfo.Parser, new[]{ "AppBundleId", "AndroidPackage", "VerificationKeyVersion", "VerificationKeyId", "SignatureAlgorithm" }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(TemporaryExposureKey), TemporaryExposureKey.Parser, new[]{ "KeyData", "TransmissionRiskLevel", "RollingStartIntervalNumber", "RollingPeriod", "ReportType", "DaysSinceOnsetOfSymptoms" }, null, new[]{ typeof(TemporaryExposureKey.Types.ReportType) }, null, null),
                    new GeneratedClrTypeInfo(typeof(TEKSignatureList), TEKSignatureList.Parser, new[]{ "Signatures" }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(TEKSignature), TEKSignature.Parser, new[]{ "SignatureInfo", "BatchNum", "BatchSize", "Signature" }, null, null, null, null)
                }));
        }
        #endregion

    }
}