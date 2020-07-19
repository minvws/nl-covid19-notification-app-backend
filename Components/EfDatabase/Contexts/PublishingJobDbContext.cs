// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.PublishingJob;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class PublishingJobDbContext : DbContext
    {
        public PublishingJobDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<EksCreateJobInputEntity> EksInput { get; set; }
        public DbSet<EksCreateJobOutputEntity> EksOutput { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfiguration(new EksCreateJobInputEtc());
            modelBuilder.ApplyConfiguration(new EksCreateJobOutputEtc());
        }
    }
}