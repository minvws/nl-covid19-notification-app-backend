// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1
{
    public class ExposureKeySetBuilderV1 : IExposureKeySetBuilder
    {
        private const string Header = "EK Export v1    ";
        private const string ContentEntryName = "export.bin"; //Fixed
        private const string GaenSignaturesEntryName = "export.sig"; //Fixed
        private const string NlSignatureEntryName = ZippedSignedContentFormatter.SignaturesEntryName;

        private readonly IContentSigner _GaenContentSigner;
        private readonly IContentSigner _NlContentSigner;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IContentFormatter _ContentFormatter;
        private readonly IExposureKeySetHeaderInfoConfig _Config;

        public ExposureKeySetBuilderV1(
            IExposureKeySetHeaderInfoConfig headerInfoConfig,
            IContentSigner gaenContentSigner,
            IContentSigner nlContentSigner,
            IUtcDateTimeProvider dateTimeProvider, 
            IContentFormatter contentFormatter)
        {
            _GaenContentSigner = gaenContentSigner;
            _NlContentSigner = nlContentSigner;
            _DateTimeProvider = dateTimeProvider;
            _ContentFormatter = contentFormatter;
            _Config = headerInfoConfig;
        }

        public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
        {
            var securityInfo = GetGaenSignatureInfo();

            var now = _DateTimeProvider.Now();

            var content = new ExposureKeySetContentArgs
            {
                Header = Header,
                Region = "NL",
                BatchNum = 1,
                BatchSize = keys.Length,
                SignatureInfos = new[] {securityInfo},
                StartTimestamp = now.AddDays(-1).ToUnixTime(), //TODO real values?
                EndTimestamp = now.ToUnixTime(), //TODO real values?
                Keys = keys
            };

            var contentBytes = _ContentFormatter.GetBytes(content);
            
            var signatures = new ExposureKeySetSignaturesContentArgs
            {
                Items = new[]
                {
                    new ExposureKeySetSignatureContentArgs
                    {
                        SignatureInfo = securityInfo,
                        Signature = _GaenContentSigner.GetSignature(contentBytes),
                        BatchSize = content.BatchSize,
                        BatchNum = content.BatchNum
                    },
                }
            };

            var nlSig = _NlContentSigner.GetSignature(contentBytes);

            return await CreateZipArchive(contentBytes, _ContentFormatter.GetBytes(signatures), nlSig);
        }

        private SignatureInfoArgs GetGaenSignatureInfo()
            => new SignatureInfoArgs
            {
                SignatureAlgorithm = _GaenContentSigner.SignatureOid,
                VerificationKeyId = _Config.VerificationKeyId,
                VerificationKeyVersion = _Config.VerificationKeyVersion
            };

        private static async Task<byte[]> CreateZipArchive(byte[] content, byte[] gaenSig, byte[] nlSig)
        {
            await using var result = new MemoryStream();
            using (var archive = new ZipArchive(result, ZipArchiveMode.Create, true))
            {
                await WriteEntry(archive, ContentEntryName, content);
                await WriteEntry(archive, GaenSignaturesEntryName, gaenSig);
                await WriteEntry(archive, NlSignatureEntryName, nlSig);
            }

            return result.ToArray();
        }

        private static async Task WriteEntry(ZipArchive archive, string entryName, byte[] content)
        {
            await using var entryStream = archive.CreateEntry(entryName).Open();
            await using var contentStream = new MemoryStream(content);
            await contentStream.CopyToAsync(entryStream);
        }

    }

}
