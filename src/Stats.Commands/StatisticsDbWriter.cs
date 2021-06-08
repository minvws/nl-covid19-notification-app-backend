// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using System.Linq;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Core;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Entities;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Commands
{
    public class StatisticsDbWriter : IStatisticsWriter
    {
        private readonly StatsDbContext _dbContext;
        private readonly IUtcDateTimeProvider _dtp;

        public StatisticsDbWriter(StatsDbContext dbContext, IUtcDateTimeProvider dtp)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dtp = dtp ?? throw new ArgumentNullException(nameof(dtp));
        }

        public void Write(StatisticArgs[] args)
        {
            var entries = args.Select(Map).ToArray();
            _dbContext.StatisticsEntries.AddRange(entries);
            _dbContext.SaveChanges(); //Implicit TX
        }

        StatisticsEntryEntity Map(StatisticArgs args)
        {
            return new StatisticsEntryEntity
            {
                Created = _dtp.Snapshot,
                Name = args.Name,
                Qualifier = args.Qualifier,
                Value = args.Value
            };
        }
    }
}