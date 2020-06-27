// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Google.Protobuf.Reflection;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat
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
            byte[] descriptorData = global::System.Convert.FromBase64String(
                string.Concat(
                    "CiBUZW1wb3JhcnlFeHBvc3VyZUtleUV4cG9ydC5wcm90byL+AQoaVGVtcG9y",
                    "YXJ5RXhwb3N1cmVLZXlFeHBvcnQSFwoPc3RhcnRfdGltZXN0YW1wGAEgASgG",
                    "EhUKDWVuZF90aW1lc3RhbXAYAiABKAYSDgoGcmVnaW9uGAMgASgJEhEKCWJh",
                    "dGNoX251bRgEIAEoBRISCgpiYXRjaF9zaXplGAUgASgFEicKD3NpZ25hdHVy",
                    "ZV9pbmZvcxgGIAMoCzIOLlNpZ25hdHVyZUluZm8SIwoEa2V5cxgHIAMoCzIV",
                    "LlRlbXBvcmFyeUV4cG9zdXJlS2V5EisKDHJldmlzZWRfa2V5cxgIIAMoCzIV",
                    "LlRlbXBvcmFyeUV4cG9zdXJlS2V5IpcBCg1TaWduYXR1cmVJbmZvEiAKGHZl",
                    "cmlmaWNhdGlvbl9rZXlfdmVyc2lvbhgDIAEoCRIbChN2ZXJpZmljYXRpb25f",
                    "a2V5X2lkGAQgASgJEhsKE3NpZ25hdHVyZV9hbGdvcml0aG0YBSABKAlKBAgB",
                    "EAJKBAgCEANSDWFwcF9idW5kbGVfaWRSD2FuZHJvaWRfcGFja2FnZSLsAgoU",
                    "VGVtcG9yYXJ5RXhwb3N1cmVLZXkSEAoIa2V5X2RhdGEYASABKAwSIwoXdHJh",
                    "bnNtaXNzaW9uX3Jpc2tfbGV2ZWwYAiABKAVCAhgBEiUKHXJvbGxpbmdfc3Rh",
                    "cnRfaW50ZXJ2YWxfbnVtYmVyGAMgASgFEhsKDnJvbGxpbmdfcGVyaW9kGAQg",
                    "ASgFOgMxNDQSNQoLcmVwb3J0X3R5cGUYBSABKA4yIC5UZW1wb3JhcnlFeHBv",
                    "c3VyZUtleS5SZXBvcnRUeXBlEiQKHGRheXNfc2luY2Vfb25zZXRfb2Zfc3lt",
                    "cHRvbXMYBiABKBEifAoKUmVwb3J0VHlwZRILCgdVTktOT1dOEAASEgoOQ09O",
                    "RklSTUVEX1RFU1QQARIgChxDT05GSVJNRURfQ0xJTklDQUxfRElBR05PU0lT",
                    "EAISDwoLU0VMRl9SRVBPUlQQAxINCglSRUNVUlNJVkUQBBILCgdSRVZPS0VE",
                    "EAUiNQoQVEVLU2lnbmF0dXJlTGlzdBIhCgpzaWduYXR1cmVzGAEgAygLMg0u",
                    "VEVLU2lnbmF0dXJlInAKDFRFS1NpZ25hdHVyZRImCg5zaWduYXR1cmVfaW5m",
                    "bxgBIAEoCzIOLlNpZ25hdHVyZUluZm8SEQoJYmF0Y2hfbnVtGAIgASgFEhIK",
                    "CmJhdGNoX3NpemUYAyABKAUSEQoJc2lnbmF0dXJlGAQgASgM"));
            descriptor = FileDescriptor.FromGeneratedCode(descriptorData,
                new FileDescriptor[] { },
                new GeneratedClrTypeInfo(null, null, new GeneratedClrTypeInfo[] {
                    new GeneratedClrTypeInfo(typeof(global::TemporaryExposureKeyExport), global::TemporaryExposureKeyExport.Parser, new[]{ "StartTimestamp", "EndTimestamp", "Region", "BatchNum", "BatchSize", "SignatureInfos", "Keys", "RevisedKeys" }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.SignatureInfo), global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.SignatureInfo.Parser, new[]{ "VerificationKeyVersion", "VerificationKeyId", "SignatureAlgorithm" }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey), global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Parser, new[]{ "KeyData", "TransmissionRiskLevel", "RollingStartIntervalNumber", "RollingPeriod", "ReportType", "DaysSinceOnsetOfSymptoms" }, null, new[]{ typeof(global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TemporaryExposureKey.Types.ReportType) }, null, null),
                    new GeneratedClrTypeInfo(typeof(global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TEKSignatureList), global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TEKSignatureList.Parser, new[]{ "Signatures" }, null, null, null, null),
                    new GeneratedClrTypeInfo(typeof(global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TEKSignature), global::NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat.TEKSignature.Parser, new[]{ "SignatureInfo", "BatchNum", "BatchSize", "Signature" }, null, null, null, null)
                }));
        }
        #endregion

    }
}