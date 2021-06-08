// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core.EntityFramework;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Domain;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Commands
{
    public class RemoveExpiredWorkflowsCommand
    {
        private readonly Func<WorkflowDbContext> _dbContextProvider;
        private readonly ExpiredWorkflowLoggingExtensions _logger;
        private readonly IUtcDateTimeProvider _dtp;
        private RemoveExpiredWorkflowsResult _result;
        private readonly IWorkflowConfig _config;

        public RemoveExpiredWorkflowsCommand(Func<WorkflowDbContext> dbContext, ExpiredWorkflowLoggingExtensions logger, IUtcDateTimeProvider dtp, IWorkflowConfig config)
        {
            _dbContextProvider = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private void ReadStats(WorkflowStats stats, WorkflowDbContext dbc)
        {
            stats.Count = dbc.KeyReleaseWorkflowStates.Count();

            stats.Expired = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _dtp.Snapshot);
            stats.Unauthorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _dtp.Snapshot && (x.LabConfirmationId != null || x.GGDKey != null) && x.AuthorisedByCaregiver == null && x.StartDateOfTekInclusion == null);
            stats.Authorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _dtp.Snapshot && x.LabConfirmationId == null && x.GGDKey == null && x.AuthorisedByCaregiver != null && x.StartDateOfTekInclusion != null);

            stats.AuthorisedAndFullyPublished = dbc.KeyReleaseWorkflowStates.Count(x =>
                x.ValidUntil < _dtp.Snapshot &&
                x.AuthorisedByCaregiver != null &&
                x.StartDateOfTekInclusion != null &&
                x.LabConfirmationId == null && x.GGDKey == null &&
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

            _logger.WriteReport(sb.ToString());
        }


        /// <summary>
        /// Delete all Workflows and their associated TEKs that are over 2 days old
        /// Cascading delete kills the TEKs.
        /// </summary>
        public RemoveExpiredWorkflowsResult Execute()
        {
            if (_result != null)
            {
                throw new InvalidOperationException("Object already used.");
            }


            _result = new RemoveExpiredWorkflowsResult();
            _result.DeletionsOn = _config.CleanupDeletesData;

            _logger.WriteStart();

            using (var dbc = _dbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    ReadStats(_result.Before, dbc);
                    LogReport(_result.Before, "Workflow stats before cleanup:");

                    if (!_result.DeletionsOn)
                    {
                        _logger.WriteFinishedNothingRemoved();
                        return _result;
                    }

                    if (_result.Before.Authorised != _result.Before.AuthorisedAndFullyPublished)
                    {
                        _logger.WriteUnpublishedTekFound();
                        throw new InvalidOperationException("Authorised unpublished TEKs exist. Aborting workflow cleanup.");
                    }

                    _result.GivenMercy = dbc.Database.ExecuteSqlInterpolated($"DELETE FROM TekReleaseWorkflowState WHERE [ValidUntil] < {_dtp.Snapshot}");
                    _logger.WriteRemovedAmount(_result.GivenMercy);
                    tx.Commit();
                }

                using (dbc.BeginTransaction())
                {
                    ReadStats(_result.After, dbc);
                }

                LogReport(_result.Before, "Workflow stats after cleanup:");
                _logger.WriteFinished();
                return _result;
            }
        }
    }
}
