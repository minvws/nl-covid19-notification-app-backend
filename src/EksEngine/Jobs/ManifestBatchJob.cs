// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs
{
    public class ManifestBatchJob : IJob
    {
        private readonly ILogger<ManifestBatchJob> _logger;
        private readonly CommandInvoker _commandInvoker;
        private readonly ManifestUpdateCommand _manifestUpdateCommand;

        public ManifestBatchJob(ILogger<ManifestBatchJob> logger,
            CommandInvoker commandInvoker,
            ManifestUpdateCommand manifestUpdateCommand
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _manifestUpdateCommand = manifestUpdateCommand ?? throw new ArgumentNullException(nameof(manifestUpdateCommand));
        }

        public void Run()
        {
            _logger.LogInformation("Manifest Batch Job started");

            _commandInvoker
                .SetCommand(_manifestUpdateCommand)
                .Execute();

            _logger.LogInformation("Manifest Batch Job finished");
        }
    }
}
