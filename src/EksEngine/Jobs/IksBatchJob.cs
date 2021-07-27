// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Outbound;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Jobs
{
    public class IksBatchJob : IJob
    {
        private readonly ILogger<IksBatchJob> _logger;
        private readonly CommandInvoker _commandInvoker;
        private readonly IksEngine _iksEngine;

        public IksBatchJob(ILogger<IksBatchJob> logger,
            CommandInvoker commandInvoker,
            IksEngine iksEngine
        )
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _iksEngine = iksEngine ?? throw new ArgumentNullException(nameof(iksEngine));
        }

        public void Run()
        {
            _logger.LogInformation("Iks Batch Job started");

            _commandInvoker
                .SetCommand(_iksEngine)
                .Execute();

            _logger.LogInformation("Iks Batch Job finished");
        }
    }
}
