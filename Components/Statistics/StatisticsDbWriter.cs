using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Services;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Statistics
{
    public class StatisticsDbWriter : IStatisticsWriter
    {
        private readonly StatsDbContext _DbContext;
        private readonly IUtcDateTimeProvider _Dtp;

        public StatisticsDbWriter(StatsDbContext dbContext, IUtcDateTimeProvider dtp)
        {
            _DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _Dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
        }

        public void Write(StatisticArgs[] args)
        {
            var entries = args.Select(Map).ToArray();
            _DbContext.StatisticsEntries.AddRange(entries);
            _DbContext.SaveAndCommit();
        }

        StatisticsEntryEntity Map(StatisticArgs args)
        {
            return new StatisticsEntryEntity
            { 
                Created = _Dtp.Snapshot,
                Name = args.Name,
                Qualifier = args.Qualifier,
                Value = args.Value
            };
        }
    }
}