// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow.Expiry
{
    public class RemoveExpiredWorkflowsCommand
    {
        private readonly Func<WorkflowDbContext> _DbContextProvider;
        private readonly ILogger<RemoveExpiredWorkflowsCommand> _Logger;
        private readonly IUtcDateTimeProvider _Dtp;
        private RemoveExpiredWorkflowsResult _Result;
        private readonly IWorkflowConfig _Config;

        public RemoveExpiredWorkflowsCommand(Func<WorkflowDbContext> dbContext, ILogger<RemoveExpiredWorkflowsCommand> logger, IUtcDateTimeProvider dtp, IWorkflowConfig config)
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
            stats.Unauthorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _Dtp.Snapshot && x.LabConfirmationId != null && x.AuthorisedByCaregiver == null && x.DateOfSymptomsOnset == null);
            stats.Authorised = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _Dtp.Snapshot && x.LabConfirmationId == null && x.AuthorisedByCaregiver != null && x.DateOfSymptomsOnset != null);

            stats.AuthorisedAndFullyPublished = dbc.KeyReleaseWorkflowStates.Count(x => x.ValidUntil < _Dtp.Snapshot
                                                                                        && x.AuthorisedByCaregiver != null
                                                                                        && x.DateOfSymptomsOnset != null
                                                                                        && x.LabConfirmationId == null
                                                                                        && x.Teks.Count(y => y.PublishingState == PublishingState.Unpublished) == 0);

            stats.TekCount = dbc.TemporaryExposureKeys.Count();
            stats.TekPublished = dbc.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published);
            stats.TekUnpublished = dbc.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Unpublished);
        }

        private void Log(WorkflowStats stats, string message)
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

            _Logger.LogInformation(sb.ToString());
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

            _Logger.LogInformation("Begin Workflow cleanup.");
            _Logger.LogInformation("Workflow cleanup complete.");

            using (var dbc = _DbContextProvider())
            {
                using (var tx = dbc.BeginTransaction())
                {
                    ReadStats(_Result.Before, dbc);
                    Log(_Result.Before, "Workflow stats before cleanup:");

                    if (!_Result.DeletionsOn)
                    {
                        _Logger.LogInformation("No Workflows deleted - Deletions switched off");
                        return _Result;
                    }

                    _Result.GivenMercy = dbc.Database.ExecuteSqlInterpolated($"WITH Zombies AS (SELECT Id FROM [TekReleaseWorkflowState] WHERE [CREATED] < DATEADD(DAY, -2, GETDATE())) DELETE Zombies");
                    _Logger.LogInformation("Workflows deleted - {GivenMercy}", _Result.GivenMercy);
                    tx.Commit();
                }

                using (dbc.BeginTransaction())
                    ReadStats(_Result.After, dbc);

                Log(_Result.Before, "Workflow stats after cleanup:");
                return _Result;
            }
        }
    }
}