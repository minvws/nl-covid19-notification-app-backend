// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.IO.Compression;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Protobuf;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public static class ProtobufHeaderCleaner
    {
        // Currently, GAEN uses ASN.1-encoding for its signature in the header.
        // This encoding adds at least two bytes to the header.
        // OpenSSL and .Net, however, don't expect this encoding and will throw a parse error.
        // To overcome this discrepancy, use this method, which omits the bytes from the header.
        // For more info, see X962PackagingFix.cs in this solution.

        public static byte[] RemoveExcessBytes(ZipArchive eksZip)
        {
            var gaenSigEntry = eksZip.GetEntry(ZippedContentEntryNames.EksGaenSig);
            using var entryStream = gaenSigEntry.Open();

            var signatureData = TEKSignatureList.Parser.ParseFrom(entryStream);
            return signatureData.Signatures[0].Signature.ToByteArray();
        }
    }
}
