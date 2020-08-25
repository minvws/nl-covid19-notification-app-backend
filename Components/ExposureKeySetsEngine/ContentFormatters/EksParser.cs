using System.IO;
using System.IO.Compression;
using NL.Rijksoverheid.ExposureNotification.BackEnd.GeneratedGaenFormat;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters
{
    public class EksParser
    {
        private const string ContentEntryName = "export.bin";
        private const string GaenSignaturesEntryName = "export.sig";
        public byte[] ReadGaenSig(string filename)
        {
            using var zipStream = File.Open(filename, FileMode.Open);
            using var zip = new ZipArchive(zipStream);
            var entry = zip.GetEntry(GaenSignaturesEntryName);
            using var entryStream = entry.Open();
            var list = TEKSignatureList.Parser.ParseFrom(entryStream);
            return list.Signatures[0].Signature.ToByteArray();
        }
        public byte[] ReadContent(string filename)
        {
            using var zipStream = File.Open(filename, FileMode.Open);
            using var zip = new ZipArchive(zipStream);
            var entry = zip.GetEntry(ContentEntryName);
            using var result = new MemoryStream();
            using var entryStream = entry.Open();
            entryStream.CopyTo(result);
            return result.ToArray();
        }
   }
}