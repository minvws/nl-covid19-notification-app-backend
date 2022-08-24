// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.DiagnosisKeys;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Eks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Iks;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Workflow;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Jobs
{
    public class DailyCleanupJob : IJob
    {
        private readonly ILogger _logger;
        private readonly CommandInvoker _commandInvoker;
        private readonly StatisticsCommand _statisticsCommand;
        private readonly RemoveExpiredWorkflowsCommand _removeExpiredWorkflowsCommand;
        private readonly RemoveDiagnosisKeysReadyForCleanupCommand _removeDiagnosisKeysReadyForCleanupCommand;
        private readonly RemovePublishedDiagnosisKeysCommand _removePublishedDiagnosisKeysCommand;
        private readonly RemoveExpiredEksCommand _removeExpiredEksCommand;
        private readonly RemoveExpiredManifestsCommand _removeExpiredManifestsCommand;
        private readonly RemoveExpiredIksInCommand _removeExpiredIksInCommand;
        private readonly RemoveExpiredIksOutCommand _removeExpiredIksOutCommand;

        public DailyCleanupJob(ILogger<DailyCleanupJob> logger,
            CommandInvoker commandInvoker,
            StatisticsCommand statisticsCommand,
            RemoveExpiredWorkflowsCommand removeExpiredWorkflowsCommand,
            RemovePublishedDiagnosisKeysCommand removePublishedDiagnosisKeysCommand,
            RemoveDiagnosisKeysReadyForCleanupCommand removeDiagnosisKeysReadyForCleanupCommand,
            RemoveExpiredEksCommand removeExpiredEksCommand,
            RemoveExpiredManifestsCommand removeExpiredManifestsCommand,
            RemoveExpiredIksInCommand removeExpiredIksInCommand,
            RemoveExpiredIksOutCommand removeExpiredIksOutCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commandInvoker = commandInvoker ?? throw new ArgumentNullException(nameof(commandInvoker));
            _statisticsCommand = statisticsCommand ?? throw new ArgumentNullException(nameof(statisticsCommand));
            _removeExpiredWorkflowsCommand = removeExpiredWorkflowsCommand ?? throw new ArgumentNullException(nameof(removeExpiredWorkflowsCommand));
            _removeDiagnosisKeysReadyForCleanupCommand = removeDiagnosisKeysReadyForCleanupCommand ?? throw new ArgumentNullException(nameof(removeDiagnosisKeysReadyForCleanupCommand));
            _removePublishedDiagnosisKeysCommand = removePublishedDiagnosisKeysCommand ?? throw new ArgumentNullException(nameof(removePublishedDiagnosisKeysCommand));
            _removeExpiredEksCommand = removeExpiredEksCommand ?? throw new ArgumentNullException(nameof(removeExpiredEksCommand));
            _removeExpiredManifestsCommand = removeExpiredManifestsCommand ?? throw new ArgumentNullException(nameof(removeExpiredManifestsCommand));
            _removeExpiredIksInCommand = removeExpiredIksInCommand ?? throw new ArgumentNullException(nameof(removeExpiredIksInCommand));
            _removeExpiredIksOutCommand = removeExpiredIksOutCommand ?? throw new ArgumentNullException(nameof(removeExpiredIksOutCommand));
        }
        public void Run()
        {
            _logger.LogInformation("Daily cleanup - Starting");

            _commandInvoker
                .SetCommand(_removeExpiredWorkflowsCommand).WithPreExecuteAction(() => _logger.LogInformation("Daily cleanup - Cleanup Workflows run starting"))
                .SetCommand(_removeDiagnosisKeysReadyForCleanupCommand)
                .SetCommand(_removePublishedDiagnosisKeysCommand)
                .SetCommand(_removeExpiredEksCommand).WithPreExecuteAction(() => _logger.LogInformation("Daily cleanup - Cleanup EKS run starting"))
                .SetCommand(_removeExpiredManifestsCommand).WithPreExecuteAction(() => _logger.LogInformation("Daily cleanup - Cleanup Manifests run starting"))
                .SetCommand(_removeExpiredIksInCommand).WithPreExecuteAction(() => _logger.LogInformation("Daily cleanup - Cleanup Expired IksIn run starting"))
                .SetCommand(_removeExpiredIksOutCommand).WithPreExecuteAction(() => _logger.LogInformation("Daily cleanup - Cleanup Expired IksOut run starting"))
                .SetCommand(_statisticsCommand).WithPreExecuteAction(() => _logger.LogInformation("Daily cleanup - Calculating daily stats starting"))
                .Execute();

            _logger.LogInformation("Daily cleanup - Finished");
        }
    }
}
