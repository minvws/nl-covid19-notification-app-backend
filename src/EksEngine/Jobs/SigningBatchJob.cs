// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs
{
    public class SigningBatchJob : IJob
    {
        private readonly ILogger<SigningBatchJob> _logger;
        private readonly CommandInvoker _commandInvoker;
        private readonly NlContentResignExistingV1ContentCommand _nlContentResignExistingV1ContentCommand;

        public SigningBatchJob(ILogger<SigningBatchJob> logger,
            CommandInvoker commandInvoker,
            NlContentResignExistingV1ContentCommand nlContentResignExistingV1ContentCommand
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _nlContentResignExistingV1ContentCommand = nlContentResignExistingV1ContentCommand ?? throw new ArgumentNullException(nameof(nlContentResignExistingV1ContentCommand));
        }

        public void Run()
        {
            _logger.LogInformation("Signing Batch Job started");

            _commandInvoker
                .SetCommand(_nlContentResignExistingV1ContentCommand)
                .Execute();

            _logger.LogInformation("Signing Batch Job finished");
        }
    }
}
