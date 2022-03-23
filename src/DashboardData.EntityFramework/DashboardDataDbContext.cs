// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DashboardData.EntityFramework
{
    public class DashboardDataDbContext : DbContext
    {
        public DashboardDataDbContext(DbContextOptions<DashboardDataDbContext> options)
            : base(options)
        {
        }

        public DbSet<DashboardInputJsonEntity> DashboardInputJson { get; set; }
        public DbSet<CdnStatsEntity> CdnStats { get; set; }
        public DbSet<DashboardOutputJsonEntity> DashboardOutputJson { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<DashboardInputJsonEntity>().HasIndex(u => u.Hash);
        }
    }
}
