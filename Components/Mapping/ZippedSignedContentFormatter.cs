// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services.Signing.Signers;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping
{
    public class ZippedSignedContentFormatter
    {
        internal const string ContentEntryName = "content.bin";
        internal const string SignaturesEntryName = "content.sig";

        private readonly IContentSigner _ContentSigner;

        public ZippedSignedContentFormatter(IContentSigner contentSigner)
        {
            _ContentSigner = contentSigner;
        }

        public async Task<byte[]> SignedContentPacket(byte[] content)
        {
            var signature = _ContentSigner.GetSignature(content);
            return await CreateZipArchive(content, signature);
        }

        private static async Task<byte[]> CreateZipArchive(byte[] content, byte[] signature)
        {
            await using var result = new MemoryStream();
            using (var archive = new ZipArchive(result, ZipArchiveMode.Create, true))
            {
                await WriteEntry(archive, ContentEntryName, content);
                await WriteEntry(archive, SignaturesEntryName, signature);
            }

            return result.ToArray();
        }

        private static async Task WriteEntry(ZipArchive archive, string entryName, byte[] content)
        {
            await using var entryStream = archive.CreateEntry(entryName).Open();
            await using var contentStream = new MemoryStream(content);
            await contentStream.CopyToAsync(entryStream);
        }

        public static async Task<byte[]> Read(byte[] content)
        {
            await using var cs = new MemoryStream(content);
            using (var archive = new ZipArchive(cs, ZipArchiveMode.Read, false))
            {
                await using var result = new MemoryStream(content);
                var entryStream = archive.GetEntry(ContentEntryName);
                entryStream.Open().CopyTo(result);
                return result.ToArray();
            }
        }
    }
}