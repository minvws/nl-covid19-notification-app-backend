// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs
{
    public class EksBatchJob : IJob
    {
        private readonly ILogger<EksBatchJob> _logger;
        private readonly CommandInvoker _commandInvoker;
        private readonly SnapshotWorkflowTeksToDksCommand _snapshotWorkflowTeksToDksCommand;
        private readonly ExposureKeySetBatchJobMk3 _exposureKeySetBatchJobMk3;

        public EksBatchJob(ILogger<EksBatchJob> logger,
            CommandInvoker commandInvoker,
            SnapshotWorkflowTeksToDksCommand snapshotWorkflowTeksToDksCommand,
            ExposureKeySetBatchJobMk3 exposureKeySetBatchJobMk3
            )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _snapshotWorkflowTeksToDksCommand = snapshotWorkflowTeksToDksCommand ?? throw new ArgumentNullException(nameof(snapshotWorkflowTeksToDksCommand));
            _exposureKeySetBatchJobMk3 = exposureKeySetBatchJobMk3 ?? throw new ArgumentNullException(nameof(exposureKeySetBatchJobMk3));
        }

        public void Run()
        {
            _logger.LogInformation("Eks Batch Job started");

            _commandInvoker
                .SetCommand(_snapshotWorkflowTeksToDksCommand)
                .SetCommand(_exposureKeySetBatchJobMk3)
                .Execute();

            _logger.LogInformation("Eks Batch Job finished");
        }
    }
}
