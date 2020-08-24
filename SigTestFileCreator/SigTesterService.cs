namespace SigTestFileCreator
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine.FormatV1;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Framework;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;
    using System.IO;
    using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Mapping;

    public sealed class SigTesterService
    {
        private readonly IEksBuilder _EksZipBuilder;
        private readonly ZippedSignedContentFormatter _ManifestZipBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly ILogger _Logger;

        private byte[] _fileContents;
        private string _fileInputLocation;
        private string _eksFileOutputLocation;
        private string _manifestFileOutputLocation;

        public SigTesterService(
            IEksBuilder eksZipBuilder,
            ZippedSignedContentFormatter manifestZipBuilder,
            IUtcDateTimeProvider dateTimeProvider,
            ILogger<SigTesterService> logger
            )
        {
            _EksZipBuilder = eksZipBuilder ?? throw new ArgumentNullException(nameof(eksZipBuilder));
            _ManifestZipBuilder = manifestZipBuilder ?? throw new ArgumentNullException(nameof(manifestZipBuilder));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _fileInputLocation = @"H:\test.txt";
            _eksFileOutputLocation = @"H:\testresult-eks.zip";
            _manifestFileOutputLocation = @"H:\testresult-manifest.zip";
        }

        public async Task Execute(string[] args)
        {
            _Logger.LogDebug("Key presence Test started ({time})", _DateTimeProvider.Snapshot);

            if (args.Length > 1)
            {
                throw new ArgumentException("The tester was started with more than one argument: ", String.Join(";", args));
            }
            else if (args.Length == 1)
            {
                string CleanedInput = args[0].Trim();
                string FilePathWithoutExtension = CleanedInput.Substring(0, CleanedInput.LastIndexOf('.'));

                _fileInputLocation = CleanedInput;
                _eksFileOutputLocation = FilePathWithoutExtension + "-eks" + ".Zip";
                _manifestFileOutputLocation = FilePathWithoutExtension + "-manifest" + ".Zip";
            }

            if (Environment.UserInteractive && !WindowsIdentityStuff.CurrentUserIsAdministrator())
                _Logger.LogError("The test was started WITHOUT elevated privileges - errors may occur when signing content.");

            LoadFile(_fileInputLocation);
            var eksZipOutput = await BuildEksOutput();
            var manifestZipOutput = await BuildManifestOutput();
            
            _Logger.LogDebug("Saving EKS-engine resultfile.");
            ExportOutput(_eksFileOutputLocation, eksZipOutput);
            
            _Logger.LogDebug("Saving Manifest-engine resultfile.");
            ExportOutput(_manifestFileOutputLocation, manifestZipOutput);

            _Logger.LogDebug("Key presence test complete.\nResults can be found in: {_fileOutputLocation}", _eksFileOutputLocation);
        }

        private void LoadFile(string filename)
        {
            try
            {
                _fileContents = File.ReadAllBytes(filename);
            }
            catch (Exception e)
            {
                throw new IOException("Something went wrong with reading the test file: ", e);
            }
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

        private async Task<byte[]> BuildEksOutput()
        {
            _Logger.LogDebug("Building EKS-engine resultfile.");

            var args = new TemporaryExposureKeyArgs[]
            {
                new TemporaryExposureKeyArgs{
                    RollingPeriod = default(int),
                    TransmissionRiskLevel = default(TransmissionRiskLevel),
                    KeyData = _fileContents,
                    RollingStartNumber = default(int)
                }
            };
            
            return await _EksZipBuilder.BuildAsync(args);
        }

        private async Task<byte[]> BuildManifestOutput()
        {
            _Logger.LogDebug("Building Manifest-engine resultfile.");
            return await _ManifestZipBuilder.SignedContentPacket(_fileContents);
        }
    }
}
