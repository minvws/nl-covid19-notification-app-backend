// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.DiagnosisKeys.EntityFramework
{
    public class DkSourceDbContext : DbContext
    {
        public DkSourceDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<DiagnosisKeyInputEntity> DiagnosisKeysInput { get; set; }
        public DbSet<DiagnosisKeyEntity> DiagnosisKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<DiagnosisKeyEntity>().OwnsOne(p => p.Local);
            modelBuilder.Entity<DiagnosisKeyEntity>().OwnsOne(p => p.Efgs);
            modelBuilder.Entity<DiagnosisKeyEntity>().OwnsOne(p => p.DailyKey);

            modelBuilder.Entity<DiagnosisKeyInputEntity>().OwnsOne(p => p.Local);
            modelBuilder.Entity<DiagnosisKeyInputEntity>().OwnsOne(p => p.DailyKey);
        }
    }
}
