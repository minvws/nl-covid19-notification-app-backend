// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<ExposureKeySetBuilderV1> _Logger;

        public ExposureKeySetBuilderV1(
            IExposureKeySetHeaderInfoConfig headerInfoConfig,
            IContentSigner gaenContentSigner,
            IContentSigner nlContentSigner,
            IUtcDateTimeProvider dateTimeProvider,
            IContentFormatter contentFormatter,
            ILogger<ExposureKeySetBuilderV1> logger
            )
        {
            _GaenContentSigner = gaenContentSigner ?? throw new ArgumentNullException(nameof(gaenContentSigner));
            _NlContentSigner = nlContentSigner ?? throw new ArgumentNullException(nameof(nlContentSigner));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ContentFormatter = contentFormatter ?? throw new ArgumentNullException(nameof(contentFormatter));
            _Config = headerInfoConfig ?? throw new ArgumentNullException(nameof(headerInfoConfig));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<byte[]> BuildAsync(TemporaryExposureKeyArgs[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (keys.Any(x => x == null)) throw new ArgumentException("At least one key in null.", nameof(keys));

            var securityInfo = GetGaenSignatureInfo();

            var now = _DateTimeProvider.Now();

            var content = new ExposureKeySetContentArgs
            {
                Header = Header,
                Region = "NL",
                BatchNum = 1,
                BatchSize = 1,
                SignatureInfos = new[] {securityInfo},
                StartTimestamp = now.AddDays(-1).ToUnixTime(), //TODO real values?
                EndTimestamp = now.ToUnixTime(), //TODO real values?
                Keys = keys
            };

            var contentBytes = _ContentFormatter.GetBytes(content);
            var nlSig = _NlContentSigner.GetSignature(contentBytes);
            var gaenSig = _GaenContentSigner.GetSignature(contentBytes);

            _Logger.LogDebug($"GAEN Sig: {Convert.ToBase64String(gaenSig)}.");
            _Logger.LogDebug($"NL Sig: {Convert.ToBase64String(nlSig)}.");

            var signatures = new ExposureKeySetSignaturesContentArgs
            {
                Items = new[]
                {
                    new ExposureKeySetSignatureContentArgs
                    {
                        SignatureInfo = securityInfo,
                        Signature = gaenSig,
                        BatchSize = content.BatchSize,
                        BatchNum = content.BatchNum
                    },
                }
            };

            var gaenSigFile = _ContentFormatter.GetBytes(signatures);
            return await CreateZipArchive(contentBytes, gaenSigFile, nlSig);
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
