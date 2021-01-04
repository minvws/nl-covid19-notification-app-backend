// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands
{
    public static class ZipArchiveExtensions
    {
        public static byte[] ReadEntry(this ZipArchive zip, string entryName)
        {
            using var entryStream = zip.GetEntry(entryName)?.Open();
            if (entryStream == null) throw new InvalidOperationException("Entry not found.");
            using var result = new MemoryStream();
            entryStream?.CopyTo(result);
            return result.ToArray();
        }
        
        public static async Task ReplaceEntryAsync(this ZipArchive archive, string entryName, byte[] zippedContent)
        {
            archive.GetEntry(entryName)?.Delete();
            await WriteEntryAsync(archive, entryName, zippedContent);
        }

        public static async Task WriteEntryAsync(this ZipArchive archive, string entryName, byte[] content)
        {
            await using var entryStream = archive.CreateEntry(entryName).Open();
            await using var contentStream = new MemoryStream(content);
            await contentStream.CopyToAsync(entryStream);
        }
    }
}