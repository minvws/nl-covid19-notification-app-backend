// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class TotalWorkflowAuthorisedCountStatsQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _DbContext;

        public TotalWorkflowAuthorisedCountStatsQueryCommand(WorkflowDbContext dbContext)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public const string Name = "WorkflowAuthorisedCount";

        public StatisticArgs Execute()
        {
            return new StatisticArgs
            {
                Name = Name,
                Value = _DbContext.KeyReleaseWorkflowStates.Count(x => x.LabConfirmationId == null)
            };
        }
    }
}