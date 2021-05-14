// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Eks.Publishing.EntityFramework
{
    public class EksPublishingJobDbContext : DbContext
    {
        public EksPublishingJobDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<EksCreateJobInputEntity> EksInput { get; set; }
        public DbSet<EksCreateJobOutputEntity> EksOutput { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<EksCreateJobInputEntity>().HasIndex(x => x.TransmissionRiskLevel);
            modelBuilder.Entity<EksCreateJobInputEntity>().HasIndex(x => x.KeyData);
            modelBuilder.Entity<EksCreateJobInputEntity>().HasIndex(x => x.TekId).IsUnique();
        }
    }
}   