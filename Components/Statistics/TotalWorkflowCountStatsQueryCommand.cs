using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    public class TotalWorkflowCountStatsQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _DbContext;

        public TotalWorkflowCountStatsQueryCommand(WorkflowDbContext dbContext)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public const string Name = "WorkflowCount";
        public StatisticArgs Execute()
        {
            return new StatisticArgs
            {
                Name = Name,
                Value = _DbContext.KeyReleaseWorkflowStates.Count()
            };
        }
    }
}