// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Stats.EntityFramework
{
    public class StatsDbContext : DbContext
    {
        public StatsDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<StatisticsEntryEntity> StatisticsEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<StatisticsEntryEntity>().HasIndex(u => u.Created);
            modelBuilder.Entity<StatisticsEntryEntity>().HasIndex(u => u.Name);
            modelBuilder.Entity<StatisticsEntryEntity>().HasIndex(u => u.Qualifier);
        }
    }
}
