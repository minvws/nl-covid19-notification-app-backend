// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;

namespace SigTestFileCreator
{
    public sealed class SigTesterService
    {
        private readonly IEksBuilder _EksZipBuilder;
        private readonly IUtcDateTimeProvider _DateTimeProvider;
        private readonly SigTestFileCreatorLoggingExtensions _Logger;

        private byte[] _fileContents;
        private string _fileInputLocation;
        private string _eksFileOutputLocation;

        public SigTesterService(
            IEksBuilder eksZipBuilder,
            IUtcDateTimeProvider dateTimeProvider,
            SigTestFileCreatorLoggingExtensions logger
            )
        {
            _EksZipBuilder = eksZipBuilder ?? throw new ArgumentNullException(nameof(eksZipBuilder));
            _DateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _fileInputLocation = @"H:\test.txt";
            _eksFileOutputLocation = @"H:\testresult-eks.zip";
        }

        public async Task ExecuteAsync(string[] args)
        {
            _Logger.WriteStart(_DateTimeProvider.Snapshot);

            if (args.Length > 1)
            {
                throw new ArgumentException("The tester was started with more than one argument: ", String.Join(";", args));
            }
            else if (args.Length == 1)
            {
                var CleanedInput = args[0].Trim();
                var FilePathWithoutExtension = CleanedInput.Substring(0, CleanedInput.LastIndexOf('.'));

                _fileInputLocation = CleanedInput;
                _eksFileOutputLocation = FilePathWithoutExtension + "-eks" + ".Zip";
            }

            if (Environment.UserInteractive && !WindowsIdentityQueries.CurrentUserIsAdministrator())
                _Logger.WriteNoElevatedPrivs();

            LoadFile(_fileInputLocation);
            var eksZipOutput = await BuildEksOutputAsync();
            
            _Logger.WriteSavingResultfile();
            ExportOutput(_eksFileOutputLocation, eksZipOutput);

            _Logger.WriteFinished(_eksFileOutputLocation);
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

        private async Task<byte[]> BuildEksOutputAsync()
        {
            _Logger.WriteBuildingResultFile();

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
    }
}
