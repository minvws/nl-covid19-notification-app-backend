// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.Interfaces;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.EksEngine.Commands.DiagnosisKeys.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Iks.Commands.Cleanup;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Manifest.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup
{
    public class DailyCleanupJob : IJob
    {
        private readonly DailyCleanupLoggingExtensions _logger;
        private readonly CommandInvoker _commandInvoker;
        private readonly StatisticsCommand _statisticsCommand;
        private readonly RemoveExpiredWorkflowsCommand _removeExpiredWorkflowsCommand;
        private readonly RemoveDiagnosisKeysReadyForCleanupCommand _removeDiagnosisKeysReadyForCleanupCommand;
        private readonly RemovePublishedDiagnosisKeysCommand _removePublishedDiagnosisKeysCommand;
        private readonly RemoveExpiredEksCommand _removeExpiredEksCommand;
        private readonly RemoveExpiredEksV2Command _removeExpiredEksV2Command;
        private readonly RemoveExpiredManifestsCommand _removeExpiredManifestsCommand;
        private readonly RemoveExpiredIksInCommand _removeExpiredIksInCommand;
        private readonly RemoveExpiredIksOutCommand _removeExpiredIksOutCommand;

        public DailyCleanupJob(DailyCleanupLoggingExtensions logger,
            CommandInvoker commandInvoker,
            StatisticsCommand statisticsCommand,
            RemoveExpiredWorkflowsCommand removeExpiredWorkflowsCommand,
            RemovePublishedDiagnosisKeysCommand removePublishedDiagnosisKeysCommand,
            RemoveDiagnosisKeysReadyForCleanupCommand removeDiagnosisKeysReadyForCleanupCommand,
            RemoveExpiredEksCommand removeExpiredEksCommand,
            RemoveExpiredEksV2Command removeExpiredEksV2Command,
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
            _removeExpiredEksV2Command = removeExpiredEksV2Command ?? throw new ArgumentNullException(nameof(removeExpiredEksV2Command));
            _removeExpiredManifestsCommand = removeExpiredManifestsCommand ?? throw new ArgumentNullException(nameof(removeExpiredManifestsCommand));
            _removeExpiredIksInCommand = removeExpiredIksInCommand ?? throw new ArgumentNullException(nameof(removeExpiredIksInCommand));
            _removeExpiredIksOutCommand = removeExpiredIksOutCommand ?? throw new ArgumentNullException(nameof(removeExpiredIksOutCommand));
        }
        public void Run()
        {
            _logger.WriteStart();

            _commandInvoker
                .SetCommand(_statisticsCommand).WithPreExecuteAction(_logger.WriteDailyStatsCalcStarting)
                .SetCommand(_removeExpiredWorkflowsCommand).WithPreExecuteAction(_logger.WriteWorkflowCleanupStarting)
                .SetCommand(_removeDiagnosisKeysReadyForCleanupCommand)
                .SetCommand(_removePublishedDiagnosisKeysCommand).WithPreExecuteAction(_logger.WriteEksCleanupStarting)
                .SetCommand(_removeExpiredEksCommand)
                .SetCommand(_removeExpiredEksV2Command).WithPreExecuteAction(_logger.WriteEksV2CleanupStarting)
                .SetCommand(_removeExpiredManifestsCommand).WithPreExecuteAction(_logger.WriteManiFestCleanupStarting)
                .SetCommand(_removeExpiredIksInCommand).WithPreExecuteAction(_logger.WriteExpiredIksInCleanupStarting)
                .SetCommand(_removeExpiredIksOutCommand).WithPreExecuteAction(_logger.WriteExpiredIksOutCleanupStarting)
                .Execute();

            _logger.WriteFinished();
        }
    }
}
