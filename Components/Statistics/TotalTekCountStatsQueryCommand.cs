using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    public class TotalTekCountStatsQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _DbContext;

        public TotalTekCountStatsQueryCommand(WorkflowDbContext dbContext)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public const string Name = "TekCount";

        public StatisticArgs Execute()
        {
            
            return new StatisticArgs
            {
                Name = Name,
                Value = _DbContext.TemporaryExposureKeys.Count()
            };
        }
    }
}