// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1
{
    public class ExposureKeySetBuilderV1 : IExposureKeySetBuilder
    {
        private const string Header = "EK Export v1    ";
        private const string ContentEntryName = "export.bin";
        private const string SignaturesEntryName = "export.sig";

        private readonly ISigner _Signer;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly IContentFormatter _ContentFormatter;
        private readonly IExposureKeySetHeaderInfoConfig _Config;
        public ExposureKeySetBuilderV1(IExposureKeySetHeaderInfoConfig headerInfoConfig, ISigner signer, IUtcDateTimeProvider dateTimeProvider, IContentFormatter contentFormatter)
        {
            _Signer = signer;
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
                BatchNum = 1, //TODO real values?
                BatchSize = keys.Length, //TODO real values?
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
                        Signature = _Signer.GetSignature(contentBytes),
                        BatchSize = 1,
                        BatchNum = 1
                    },
                    //new ExposureKeySetSignatureContentArgs
                    //{
                    //    //TODO The NL sig.
                    //},
                }
            };

            return await CreateZipArchive(contentBytes, _ContentFormatter.GetBytes(signatures));
        }

        //Germany has
        //signature_infos {
        //app_bundle_id: " de.rki.coronawarnapp"
        //verification_key_version: "v1"
        //verification_key_id: "262"
        //signature_algorithm: "1.2.840.10045.4.3.2"
        //}
        private SignatureInfoArgs GetGaenSignatureInfo()
            => new SignatureInfoArgs
            {
                AppBundleId = _Config.AppBundleId,
                SignatureAlgorithm = _Signer.SignatureDescription,
                VerificationKeyId = _Config.VerificationKeyId,
                VerificationKeyVersion = _Config.VerificationKeyVersion
            };

        private static async Task<byte[]> CreateZipArchive(byte[] content, byte[] signatures)
        {
            await using var result = new MemoryStream();
            using (var archive = new ZipArchive(result, ZipArchiveMode.Create, true))
            {
                await WriteEntry(archive, ContentEntryName, content);
                await WriteEntry(archive, SignaturesEntryName, signatures);
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
