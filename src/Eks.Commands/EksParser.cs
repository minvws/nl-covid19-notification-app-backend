// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO;
using System.IO.Compression;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Protobuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class EksParser
    {
        public byte[] ReadGaenSig(string filename)
        {
            using var zipStream = File.Open(filename, FileMode.Open);
            using var zip = new ZipArchive(zipStream);
            var entry = zip.GetEntry(ZippedContentEntryNames.EksGaenSig);
            using var entryStream = entry.Open();
            var list = TEKSignatureList.Parser.ParseFrom(entryStream);
            return list.Signatures[0].Signature.ToByteArray();
        }
        public byte[] ReadContent(string filename)
        {
            using var zipStream = File.Open(filename, FileMode.Open);
            using var zip = new ZipArchive(zipStream);
            return zip.ReadEntry(ZippedContentEntryNames.EksContent);
        }
   }
}