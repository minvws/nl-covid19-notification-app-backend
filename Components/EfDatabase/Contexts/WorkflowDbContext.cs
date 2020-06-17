// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<KeyReleaseWorkflowState> KeyReleaseWorkflowStates { get; set; }
        public DbSet<TemporaryExposureKeyEntity> TemporaryExposureKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new Configuration.Workflow.KeyReleaseWorkflowStateConfig());
            modelBuilder.ApplyConfiguration(new Configuration.Workflow.TemporaryExposureKeyConfig());
        }
    }

    public class WorkflowDbContextFactory : IDesignTimeDbContextFactory<WorkflowDbContext>
    {
        public WorkflowDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<WorkflowDbContext>();
            optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=WorkFlow;Integrated Security=true");

            return new WorkflowDbContext(optionsBuilder.Options);
        }
    }
}