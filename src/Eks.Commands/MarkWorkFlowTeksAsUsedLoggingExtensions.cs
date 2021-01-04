// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands
{
    public class MarkWorkFlowTeksAsUsedLoggingExtensions
    {
        private const string Name = "MarkWorkFlowTeksAsUsed";
        private const int Base = LoggingCodex.MarkWorkflowTeksAsUsed;
        private const int MarkAsPublished = Base + 1;

        private readonly ILogger _Logger;

        public MarkWorkFlowTeksAsUsedLoggingExtensions(ILogger<MarkWorkFlowTeksAsUsedLoggingExtensions> logger)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteMarkingAsPublished(int zapcount, int total)
        {
            _Logger.LogInformation("[{name}/{id}] Marking as Published - Count:{Count}, Running total:{RunningTotal}.",
                Name, MarkAsPublished,
                zapcount, total);
        }
    }
}
