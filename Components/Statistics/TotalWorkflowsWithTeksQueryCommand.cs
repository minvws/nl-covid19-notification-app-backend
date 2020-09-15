using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    public class TotalWorkflowsWithTeksQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _DbContext;

        public TotalWorkflowsWithTeksQueryCommand(WorkflowDbContext dbContext)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public const string Name = "WorkflowsWithTeksCount";
        public StatisticArgs Execute()
        {
            return new StatisticArgs
            {
                Name = "WorkflowsWithTeksCount",
                Value = _DbContext.KeyReleaseWorkflowStates.Count(x => x.Teks.Any())
            };
        }
    }
}