// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;

namespace ProtobufScrubber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Write("Error!\nUse ProtobufScrubber with a single zip that contains an export.sig file.");
                return;
            }

            var cleanedInput = Path.GetFullPath(args[0].Trim());
            try
            {
                ScrubZipFile(cleanedInput).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                throw new Exception($"Error occurred when trying to scrub protobuf data on \"{cleanedInput}\": ", exception);
            }
        }

        private static async Task ScrubZipFile(string inputFile)
        {
            var directory = Path.GetDirectoryName(inputFile);
            var filename = Path.GetFileNameWithoutExtension(inputFile);
            var outputFileName = $"{directory}\\{filename}-scrubbed.zip";

            using (var inputZip = new ZipArchive(File.Open(inputFile, FileMode.Open)))
            {
                var scrubbedData = ProtobufHeaderCleaner.RemoveExcessBytes(inputZip);

                using (var outputZip = new ZipArchive(File.Open(outputFileName, FileMode.CreateNew), ZipArchiveMode.Create))
                {
                    foreach (var entry in inputZip.Entries)
                    {
                        if (entry.Name == ZippedContentEntryNames.EksGaenSig)
                        {
                            await outputZip.WriteEntryAsync(entry.Name, scrubbedData);
                        }
                        else
                        {
                            await outputZip.WriteEntryAsync(
                                entry.Name,
                                inputZip.ReadEntry(entry.Name));
                        }
                    }
                }
            }
        }

    }
}
