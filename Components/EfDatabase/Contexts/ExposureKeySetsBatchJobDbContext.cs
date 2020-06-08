// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class ExposureKeySetsBatchJobDbContext : DbContext
    {
        public ExposureKeySetsBatchJobDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<TeksInputEntity> Input { get; set; }
        public DbSet<ExposureKeySetEntity> Output { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Configuration.Job.WorkflowInput());
            modelBuilder.ApplyConfiguration(new Configuration.Job.ExposureKeySet());
        }
    }
}