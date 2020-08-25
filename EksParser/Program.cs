using System;
using System.IO;

namespace EksParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Write("Error!\nUsage: eksparse <eksfilename>");
                return;
            }

            var parser = new NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.ContentFormatters.EksParser();

            using (var output = File.Create("export.sig", 1024, FileOptions.None))
                output.Write(parser.ReadGaenSig(args[0]));

            using (var output = File.Create("export.bin", 1024, FileOptions.None))
                output.Write(parser.ReadContent(args[0]));
        }
    }
}
