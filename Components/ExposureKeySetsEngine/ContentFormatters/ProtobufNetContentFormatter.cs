// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
using ProtoBuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters
{
    public class ProtobufNetContentFormatter : IContentFormatter
    {
        private static byte[] GetBytes<T>(T content)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, content);
            return stream.ToArray();
        }

        public byte[] GetBytes(ExposureKeySetContentArgs content)
        {
            var result = new ExposureKeySetContent { 
                Region  = content.Region,
                BatchNum = content.BatchNum,
                BatchSize = content.BatchSize,
                EndTimestamp = content.EndTimestamp,
                Header = content.Header,
                Keys = content.Keys.Select(Map).ToArray(),
                SignatureInfos = content.SignatureInfos.Select(Map).ToArray(),
                StartTimestamp = content.StartTimestamp,
            };
            return GetBytes(result);
        }

        private ExposureKeySetKeyContent Map(TemporaryExposureKeyArgs arg)
            => new ExposureKeySetKeyContent
            {
                DailyKey = arg.KeyData,
                Risk = arg.TransmissionRiskLevel,
                RollingPeriod = arg.RollingPeriod,
                RollingStart = arg.RollingStartNumber,
            };

        private static SignatureInfo Map(SignatureInfoArgs arg)
            => new SignatureInfo
            {
                AppBundleId = arg.AppBundleId,
                SignatureAlgorithm = arg.SignatureAlgorithm,
                VerificationKeyId = arg.VerificationKeyId,
                VerificationKeyVersion = arg.VerificationKeyVersion
            };

        public byte[] GetBytes(ExposureKeySetSignaturesContentArgs arg)
        {
            var result = new ExposureKeySetSignaturesContent
            { 
                Items = arg.Items.Select(Map).ToArray()
            };
            return GetBytes(result);
        }

        private static ExposureKeySetSignatureContent Map(ExposureKeySetSignatureContentArgs args)
            => new ExposureKeySetSignatureContent 
            { 
                BatchNum = args.BatchNum,
                BatchSize = args.BatchSize,
                Signature = args.Signature,
                SignatureInfo = Map(args.SignatureInfo)
            };
    }
}