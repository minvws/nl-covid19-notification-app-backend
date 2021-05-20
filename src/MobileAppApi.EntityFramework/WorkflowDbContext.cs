// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.MobileAppApi.Workflow.EntityFramework
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<TekReleaseWorkflowStateEntity> KeyReleaseWorkflowStates { get; set; }
        public DbSet<TekEntity> TemporaryExposureKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<TekReleaseWorkflowStateEntity>().HasIndex(x => x.BucketId).IsUnique();
            modelBuilder.Entity<TekReleaseWorkflowStateEntity>().HasIndex(x => x.ConfirmationKey).IsUnique();
            modelBuilder.Entity<TekReleaseWorkflowStateEntity>().HasIndex(x => x.LabConfirmationId).IsUnique();
            modelBuilder.Entity<TekReleaseWorkflowStateEntity>().HasIndex(x => x.GGDKey).IsUnique();

            modelBuilder.Entity<TekReleaseWorkflowStateEntity>().HasIndex(x => x.ValidUntil);
            modelBuilder.Entity<TekReleaseWorkflowStateEntity>().HasIndex(x => x.AuthorisedByCaregiver);

            modelBuilder
                .Entity<TekReleaseWorkflowStateEntity>()
                .HasMany(x => x.Teks)
                .WithOne(x => x.Owner)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TekEntity>().HasIndex(u => u.PublishingState);
            modelBuilder.Entity<TekEntity>().HasIndex(u => u.PublishAfter);
        }
    }
}