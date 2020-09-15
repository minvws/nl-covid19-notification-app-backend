using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    public class PublishedTekCountStatsQueryCommand : IStatsQueryCommand
    {
        private readonly WorkflowDbContext _DbContext;

        public PublishedTekCountStatsQueryCommand(WorkflowDbContext dbContext)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public const string Name = "TekPublishedCount";

        public StatisticArgs Execute()
        {
            return new StatisticArgs
            {
                Name = Name,
                Value = _DbContext.TemporaryExposureKeys.Count(x => x.PublishingState == PublishingState.Published)
            };
        }
    }
}