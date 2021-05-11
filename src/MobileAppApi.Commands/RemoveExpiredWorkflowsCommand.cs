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
        private readonly Func<WorkflowDbContext> _DbContextProvider;
        private readonly ExpiredWorkflowLoggingExtensions _Logger;
        private readonly IUtcDateTimeProvider _Dtp;
        private RemoveExpiredWorkflowsResult _Result;
        private readonly IWorkflowConfig _Config;

        public RemoveExpiredWorkflowsCommand(Func<WorkflowDbContext> dbContext, ExpiredWorkflowLoggingExtensions logger, IUtcDateTimeProvider dtp, IWorkflowConfig config)
        {
            _DbContextProvider = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
            _Config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private void ReadStats(WorkflowStats stats, WorkflowDbContext dbc)
        {
            stats.Count = dbc.KeyReleaseWorkflowStates.Count();

            stats.Expired = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _Dtp.Snapshot);
            stats.Unauthorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _Dtp.Snapshot && x.LabConfirmationId != null && x.AuthorisedByCaregiver == null && x.StartDateOfTekInclusion == null);
            stats.Authorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _Dtp.Snapshot && x.LabConfirmationId == null && x.AuthorisedByCaregiver != null && x.StartDateOfTekInclusion != null);

            stats.AuthorisedAndFullyPublished = dbc.KeyReleaseWorkflowStates.Count(x =>
                x.ValidUntil < _Dtp.Snapshot &&
                x.AuthorisedByCaregiver != null &&
                x.StartDateOfTekInclusion != null &&
                x.LabConfirmationId == null &&
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

            _Logger.WriteReport(sb.ToString());
        }


        /// <summary>
        /// Delete all Workflows and their associated TEKs that are over 2 days old
        /// Cascading delete kills the TEKs.
        /// </summary>
        public RemoveExpiredWorkflowsResult Execute()
        {
            if (_Result != null)
                throw new InvalidOperationException("Object already used.");


            _Result = new RemoveExpiredWorkflowsResult();
            _Result.DeletionsOn = _Config.CleanupDeletesData;

            _Logger.WriteStart();

            using (var dbc = _DbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    ReadStats(_Result.Before, dbc);
                    LogReport(_Result.Before, "Workflow stats before cleanup:");

                    if (!_Result.DeletionsOn)
                    {
                        _Logger.WriteFinishedNothingRemoved();
                        return _Result;
                    }

                    if (_Result.Before.Authorised != _Result.Before.AuthorisedAndFullyPublished)
                    {
                        _Logger.WriteUnpublishedTekFound();
                        throw new InvalidOperationException("Authorised unpublished TEKs exist. Aborting workflow cleanup.");
                    }

                    _Result.GivenMercy = dbc.Database.ExecuteSqlInterpolated($"DELETE FROM TekReleaseWorkflowState WHERE [ValidUntil] < {_Dtp.Snapshot}");
                    _Logger.WriteRemovedAmount(_Result.GivenMercy);
                    tx.Commit();
                }

                using (dbc.BeginTransaction())
                    ReadStats(_Result.After, dbc);

                LogReport(_Result.Before, "Workflow stats after cleanup:");
                _Logger.WriteFinished();
                return _Result;
            }
        }
    }
}