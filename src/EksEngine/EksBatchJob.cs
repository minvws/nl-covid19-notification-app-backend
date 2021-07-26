// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine
{
    public class EksBatchJob : IJob
    {
        private readonly ILogger<EksBatchJob> _logger;
        private readonly CommandInvoker _commandInvoker;
        public EksBatchJob(ILogger<EksBatchJob> logger, CommandInvoker commandInvoker)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
        }

        public void Run()
        {

            _commandInvoker.Run();

        }
    }
}
