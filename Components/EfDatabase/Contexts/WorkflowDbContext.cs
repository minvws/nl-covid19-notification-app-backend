// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions options)
            : base(options)
        {
        }

        
        public DbSet<TekReleaseWorkflowEntity> TekReleaseWorkflowEntity { get; set; }

        [Obsolete]
        public DbSet<KeysFirstTekReleaseWorkflowEntity> KeysFirstWorkflows { get; set; }

        public DbSet<KeysLastTekReleaseWorkflowEntity> KeysLastWorkflows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Configuration.Workflow.KeysFirstTekReleaseWorkflow());
            modelBuilder.ApplyConfiguration(new Configuration.Workflow.KeysLastTekReleaseWorkflow());
            modelBuilder.ApplyConfiguration(new Configuration.Workflow.TekReleaseWorkflow());
        }
    }
}