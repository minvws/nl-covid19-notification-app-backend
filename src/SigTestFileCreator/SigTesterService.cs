// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.FormatV1;

namespace SigTestFileCreator
{
    public sealed class SigTesterService
    {
        private readonly IEksBuilder _eksZipBuilder;
        private readonly IUtcDateTimeProvider _dateTimeProvider;
        private readonly ILogger _logger;

        public SigTesterService(
            IEksBuilder eksZipBuilder,
            IUtcDateTimeProvider dateTimeProvider,
            ILogger<SigTesterService> logger
            )
        {
            _eksZipBuilder = eksZipBuilder ?? throw new ArgumentNullException(nameof(eksZipBuilder));
            _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(string[] args)
        {
            _logger.LogDebug("Key presence Test started ({Time}).", _dateTimeProvider.Snapshot);

            if (args.Length != 1)
            {
                throw new ArgumentException("The SigTestFileCreator requires one file as a command-line argument.");
            }

            var fullPath = Path.GetFullPath(args[0].Trim());
            var fileDirectory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileNameWithoutExtension(fullPath);

            var fileContents = File.ReadAllBytes(fullPath);

            var (signedFileContents, _) = await BuildEksOutputAsync(fileContents);

            _logger.LogDebug("Saving EKS-engine resultfile.");

            using (var outputFile = File.Create($"{fileDirectory}\\{fileName}-signed.zip", 1024, FileOptions.None))
            {
                outputFile.Write(signedFileContents);
            }

            _logger.LogDebug("Key presence test complete.\nResults can be found in: {OutputLocation}.",
                fileDirectory);
        }

        private async Task<(byte[], byte[])> BuildEksOutputAsync(byte[] fileData)
        {
            _logger.LogDebug("Building EKS-engine resultfile.");

            var args = new TemporaryExposureKeyArgs[]
            {
                new TemporaryExposureKeyArgs{
                    RollingPeriod = default(int),
                    TransmissionRiskLevel = default(TransmissionRiskLevel),
                    KeyData = fileData,
                    RollingStartNumber = default(int)
                }
            };

            return await _eksZipBuilder.BuildAsync(args);
        }
    }
}
