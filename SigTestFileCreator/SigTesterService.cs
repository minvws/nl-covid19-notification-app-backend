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
        private readonly List<EksCreateJobInputEntity> _Output;
        
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

            _Output = new List<EksCreateJobInputEntity>(20); //value grabbed from a mock
        }

        public async Task Execute()
        {
            _Logger.LogDebug("Key presence Test started ({time})", _DateTimeProvider.Snapshot);

            if (Environment.UserInteractive && !WindowsIdentityStuff.CurrentUserIsAdministrator())
                _Logger.LogError("The test was started WITHOUT elevated privileges - errors may occur when signing content.");

            LoadFile(_fileInputLocation);
            AddContentsToOutput();
            await BuildOutput();
            ExportOutput(_fileOutputLocation);

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

            _Output.Add(new EksCreateJobInputEntity
            {
                KeyData = contents
            });
        }

        private void AddContentsToOutput()
        {
            //meh
        }

        private void ExportOutput(string filename)
        {
            try
            {
                File.WriteAllBytes(filename, _Output.FirstOrDefault().KeyData);
            }
            catch (Exception e)
            {
                throw new IOException("Something went wrong with writing the result file: ", e);
            }
        }
        
        private async Task BuildOutput()
        {
            _Logger.LogDebug("Building resultfile.");

            var args = _Output.Select(c =>
                new TemporaryExposureKeyArgs
                {
                    RollingPeriod = c.RollingPeriod,
                    TransmissionRiskLevel = c.TransmissionRiskLevel,
                    KeyData = c.KeyData,
                    RollingStartNumber = c.RollingStartNumber
                })
                .ToArray();

            var content = await _SetBuilder.BuildAsync(args);
        }
    }
}
