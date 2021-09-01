// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Workflow
{
    public class ExpiredWorkflowLoggingExtensions
    {
        private const string Name = "RemoveExpiredWorkflow";
        private const int Base = LoggingCodex.RemoveExpiredWorkflow;

        private const int Start = Base;
        private const int Finished = Base + 99;

        private const int Report = Base + 1;
        private const int RemovedAmount = Base + 2;
        private const int UnpublishedTekFound = Base + 97;
        private const int FinishedNothingRemoved = Base + 98;

        private readonly ILogger _logger;

        public ExpiredWorkflowLoggingExtensions(ILogger<ExpiredWorkflowLoggingExtensions> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void WriteStart()
        {
            _logger.LogInformation("[{name}/{id}] Begin Workflow cleanup.",
                Name, Start);
        }

        public void WriteFinished()
        {
            _logger.LogInformation("[{name}/{id}] Workflow cleanup complete.",
                Name, Finished);
        }

        public void WriteFinishedNothingRemoved()
        {
            _logger.LogInformation("[{name}/{id}] No Workflows deleted - Deletions switched off.",
                Name, FinishedNothingRemoved);
        }

        public void WriteReport(string report)
        {
            _logger.LogInformation("[{name}/{id}] {report}.",
                Name, Report,
                report);
        }

        public void WriteUnpublishedTekFound()
        {
            _logger.LogCritical("[{name}/{id}] Authorised unpublished TEKs exist. Aborting workflow cleanup.",
                Name, UnpublishedTekFound);
        }

        public void WriteRemovedAmount(int givenMercyCount)
        {
            _logger.LogInformation("[{name}/{id}] Workflows deleted - Unauthorised:{unauthorised}",
                Name, RemovedAmount,
                givenMercyCount);
        }
    }
}
