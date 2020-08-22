namespace SigTestFileCreator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;using Microsoft.Extensions.Logging;using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
    using System.IO;

    public sealed class SigTesterService
    {
        private readonly IEksBuilder _SetBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger _Logger;
        private readonly List<EksCreateJobInputEntity> _JobData;
        
        private string _fileInputLocation;
        private string _fileOutputLocation;

        public SigTesterService(
            IEksBuilder builder,
            IUtcDateTimeProvider dateTimeProvider,
            ILogger<SigTesterService> logger
            )
        {
            _SetBuilder = builder ?? throw new ArgumentNullException(nameof(builder));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _fileInputLocation = @"H:\test.txt";
            _fileOutputLocation = @"H:\testresult.zip";

            _JobData = new List<EksCreateJobInputEntity>(20); //value grabbed from a mock
        }

        public async Task Execute(string[] args)
        {
            _Logger.LogDebug("Key presence Test started ({time})", _DateTimeProvider.Snapshot);

            if (args.Length > 1)
            {
                throw new ArgumentException("The tester was started with more than one argument: ", String.Join(";",args));
            }
            else if (args.Length == 1)
            {
                string CleanedInput = args[0].Trim();
                string FilePathWithoutExtension = CleanedInput.Substring(0, CleanedInput.LastIndexOf('.'));

                _fileInputLocation = CleanedInput;
                _fileOutputLocation = FilePathWithoutExtension + "-Signed" + ".Zip";
            }

            if (Environment.UserInteractive && !WindowsIdentityStuff.CurrentUserIsAdministrator())
                _Logger.LogError("The test was started WITHOUT elevated privileges - errors may occur when signing content.");

            LoadFile(_fileInputLocation);
            var output = await BuildOutput();
            ExportOutput(_fileOutputLocation, output);

            _Logger.LogDebug("Key presence test complete.\nResults can be found in: {_fileOutputLocation}", _fileOutputLocation);
        }

        private void LoadFile(string filename)
        {
            byte[] contents;

            try
            {
                contents = File.ReadAllBytes(filename);
            }
            catch (Exception e)
            {
                throw new IOException("Something went wrong with reading the test file: ", e);
            }

            _JobData.Add(new EksCreateJobInputEntity
            {
                KeyData = contents
            });
        }

        private void ExportOutput(string filename, byte[] output)
        {
            try
            {
                File.WriteAllBytes(filename, output);
            }
            catch (Exception e)
            {
                throw new IOException("Something went wrong with writing the result file: ", e);
            }
        }

        private async Task<byte[]> BuildOutput()
        {
            _Logger.LogDebug("Building resultfile.");

            var args = _JobData.Select(c =>
                new TemporaryExposureKeyArgs
                {
                    RollingPeriod = c.RollingPeriod,
                    TransmissionRiskLevel = c.TransmissionRiskLevel,
                    KeyData = c.KeyData,
                    RollingStartNumber = c.RollingStartNumber
                })
                .ToArray();

            return await _SetBuilder.BuildAsync(args);
        }
    }
}
