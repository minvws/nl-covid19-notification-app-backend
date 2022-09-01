// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DailyCleanup.Commands.Workflow
{
    public class RemoveExpiredWorkflowsCommand : BaseCommand
    {
        private readonly WorkflowDbContext _workflowDbContext;
        private readonly ILogger _logger;
        private readonly IUtcDateTimeProvider _dtp;
        private RemoveExpiredWorkflowsResult _result;
        private readonly IWorkflowConfig _config;

        public RemoveExpiredWorkflowsCommand(WorkflowDbContext workflowDbContext, ILogger<RemoveExpiredWorkflowsCommand> logger, IUtcDateTimeProvider dtp, IWorkflowConfig config)
        {
            _workflowDbContext = workflowDbContext ?? throw new ArgumentNullException(nameof(workflowDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Delete all Workflows and their associated TEKs that are over 2 days old
        /// Cascading delete kills the TEKs.
        /// </summary>
        public override async Task<ICommandResult> ExecuteAsync()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }

            _result = new RemoveExpiredWorkflowsResult
            {
                DeletionsOn = _config.CleanupDeletesData
            };

            _logger.LogInformation("Begin Workflow cleanup.");

            ReadStats(_result.Before, _workflowDbContext);
            LogReport(_result.Before, "Workflow stats before cleanup:");

            if (!_result.DeletionsOn)
            {
                _logger.LogInformation("No Workflows deleted - Deletions switched off.");
                return _result;
            }

            if (_result.Before.Authorised != _result.Before.AuthorisedAndFullyPublished)
            {
                // Authorised unpublished TEKs exist.
                _logger.LogCritical("Authorised unpublished TEKs exist. Aborting workflow cleanup.");
                _result.HasErrors = true;
            }
            else
            {
                var workflowsToDelete = _workflowDbContext.KeyReleaseWorkflowStates.AsNoTracking().Where(p => p.ValidUntil < _dtp.Snapshot).ToList();
                _result.GivenMercy = workflowsToDelete.Count;

                if (workflowsToDelete.Any())
                {
                    var idsToDelete = string.Join(",", workflowsToDelete.Select(x => x.Id.ToString()).ToArray());
                    await _workflowDbContext.BulkDeleteSqlRawAsync<TekReleaseWorkflowStateEntity>(idsToDelete);
                }

                _logger.LogInformation("Workflows deleted - Unauthorised: {UnauthorisedWorkflows}", _result.GivenMercy);
            }

            ReadStats(_result.After, _workflowDbContext);

            LogReport(_result.Before, "Workflow stats after cleanup:");
            _logger.LogInformation("Workflow cleanup complete.");
            return _result;
        }

        private void ReadStats(WorkflowStats stats, WorkflowDbContext dbc)
        {
            stats.Count = dbc.KeyReleaseWorkflowStates.Count();

            stats.Expired = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _dtp.Snapshot);
            stats.Unauthorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _dtp.Snapshot && x.GGDKey != null && x.AuthorisedByCaregiver == null && x.StartDateOfTekInclusion == null);
            stats.Authorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _dtp.Snapshot && x.GGDKey == null && x.AuthorisedByCaregiver != null && x.StartDateOfTekInclusion != null);

            stats.AuthorisedAndFullyPublished = dbc.KeyReleaseWorkflowStates.Count(x =>
                x.ValidUntil < _dtp.Snapshot &&
                x.AuthorisedByCaregiver != null &&
                x.StartDateOfTekInclusion != null &&
                x.GGDKey == null &&
                x.Teks.Count(y => y.PublishingState == PublishingState.Unpublished) == 0);

            stats.TekCount = dbc.TemporaryExposureKeys.Count();
            stats.TekPublished = dbc.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published);
            stats.TekUnpublished = dbc.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished);
        }

        private void LogReport(WorkflowStats stats, string message)
        {
            var sb = new StringBuilder(message);
            sb.AppendLine();
            sb.AppendLine($"{nameof(WorkflowStats.Count)}:{stats.Count}");
            sb.AppendLine($"{nameof(WorkflowStats.Expired)}:{stats.Expired}");
            sb.AppendLine("of which:");
            sb.AppendLine($"   Unauthorised:{stats.Unauthorised}");
            sb.AppendLine($"   Authorised:{stats.Authorised}");
            sb.AppendLine("    of which:");
            sb.AppendLine($"      FullyPublished:{stats.AuthorisedAndFullyPublished}");
            sb.AppendLine();
            sb.AppendLine($"{nameof(WorkflowStats.TekCount)}:{stats.TekPublished}");
            sb.AppendLine("of which:");
            sb.AppendLine($"   Published:{stats.TekPublished}");
            sb.AppendLine($"   Unpublished:{stats.TekUnpublished}");

            _logger.LogInformation("{WorkflowReport}.", sb.ToString());
        }
    }
}
