// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class TotalWorkflowsWithTeksQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _dbContext;

        public TotalWorkflowsWithTeksQueryCommand(WorkflowDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public const string Name = "WorkflowsWithTeksCount";
        public StatisticArgs Execute()
        {
            return new StatisticArgs
            {
                Name = "WorkflowsWithTeksCount",
                Value = _dbContext.KeyReleaseWorkflowStates.Count(x => x.Teks.Any())
            };
        }
    }
}
