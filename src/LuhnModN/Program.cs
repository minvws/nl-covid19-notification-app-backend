using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Icc.Commands.TekPublication;

namespace LuhnModN
{
    class Program
    {
        static void Main(string[] args)
        {
            var validator = new PublishTekArgsValidator(new StandardUtcDateTimeProvider());
            validator.PrintMatrix();

            Console.ReadKey();
        }
    }
}
