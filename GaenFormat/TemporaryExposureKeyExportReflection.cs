// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat
{
    /// <summary>Holder for reflection information generated from TemporaryExposureKeyExport.proto</summary>
    public static partial class TemporaryExposureKeyExportReflection
    {

        #region Descriptor

        /// <summary>File descriptor for TemporaryExposureKeyExport.proto</summary>
        public static FileDescriptor Descriptor
        {
            get { return descriptor; }
        }

        private static FileDescriptor descriptor;

        static TemporaryExposureKeyExportReflection()
        {
            var descriptorData = System.Convert.FromBase64String(
                string.Concat(
                    "CiBUZW1wb3JhcnlFeHBvc3VyZUtleUV4cG9ydC5wcm90byLRAQoaVGVtcG9y",
                    "YXJ5RXhwb3N1cmVLZXlFeHBvcnQSFwoPc3RhcnRfdGltZXN0YW1wGAEgASgG",
                    "EhUKDWVuZF90aW1lc3RhbXAYAiABKAYSDgoGcmVnaW9uGAMgASgJEhEKCWJh",
                    "dGNoX251bRgEIAEoBRISCgpiYXRjaF9zaXplGAUgASgFEicKD3NpZ25hdHVy",
                    "ZV9pbmZvcxgGIAMoCzIOLlNpZ25hdHVyZUluZm8SIwoEa2V5cxgHIAMoCzIV",
                    "LlRlbXBvcmFyeUV4cG9zdXJlS2V5IpsBCg1TaWduYXR1cmVJbmZvEhUKDWFw",
                    "cF9idW5kbGVfaWQYASABKAkSFwoPYW5kcm9pZF9wYWNrYWdlGAIgASgJEiAK",
                    "GHZlcmlmaWNhdGlvbl9rZXlfdmVyc2lvbhgDIAEoCRIbChN2ZXJpZmljYXRp",
                    "b25fa2V5X2lkGAQgASgJEhsKE3NpZ25hdHVyZV9hbGdvcml0aG0YBSABKAki",
                    "jQEKFFRlbXBvcmFyeUV4cG9zdXJlS2V5EhAKCGtleV9kYXRhGAEgASgMEh8K",
                    "F3RyYW5zbWlzc2lvbl9yaXNrX2xldmVsGAIgASgFEiUKHXJvbGxpbmdfc3Rh",
                    "cnRfaW50ZXJ2YWxfbnVtYmVyGAMgASgFEhsKDnJvbGxpbmdfcGVyaW9kGAQg",
                    "ASgFOgMxNDQiNQoQVEVLU2lnbmF0dXJlTGlzdBIhCgpzaWduYXR1cmVzGAEg",
                    "AygLMg0uVEVLU2lnbmF0dXJlInAKDFRFS1NpZ25hdHVyZRImCg5zaWduYXR1",
                    "cmVfaW5mbxgBIAEoCzIOLlNpZ25hdHVyZUluZm8SEQoJYmF0Y2hfbnVtGAIg",
                    "ASgFEhIKCmJhdGNoX3NpemUYAyABKAUSEQoJc2lnbmF0dXJlGAQgASgM"));
            descriptor = FileDescriptor.FromGeneratedCode(descriptorData,
                new FileDescriptor[] { },
                new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[]
                {
                    new GeneratedClrTypeInfo(typeof(TemporaryExposureKeyExport),
                        TemporaryExposureKeyExport.Parser,
                        new[]
                        {
                            "StartTimestamp", "EndTimestamp", "Region", "BatchNum", "BatchSize", "SignatureInfos",
                            "Keys"
                        }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(SignatureInfo), SignatureInfo.Parser,
                        new[]
                        {
                            "AppBundleId", "AndroidPackage", "VerificationKeyVersion", "VerificationKeyId",
                            "SignatureAlgorithm"
                        }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(TemporaryExposureKey),
                        TemporaryExposureKey.Parser,
                        new[] {"KeyData", "TransmissionRiskLevel", "RollingStartIntervalNumber", "RollingPeriod"}, null,
                        null, null, null),
                    new GeneratedClrTypeInfo(typeof(TEKSignatureList), TEKSignatureList.Parser,
                        new[] {"Signatures"}, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(TEKSignature), TEKSignature.Parser,
                        new[] {"SignatureInfo", "BatchNum", "BatchSize", "Signature"}, null, null, null, null)
                }));
        }

        #endregion

    }
}