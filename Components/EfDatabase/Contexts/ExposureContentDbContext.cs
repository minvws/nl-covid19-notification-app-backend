// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.AppConfig;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class ExposureContentDbContext : DbContext
    {
        public ExposureContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<EksCreateJobInputEntity> EksInput { get; set; }
        public DbSet<EksCreateJobOutputEntity> EksOutput { get; set; }
        public DbSet<ManifestEntity> ManifestContent { get; set; }
        public DbSet<ExposureKeySetContentEntity> ExposureKeySetContent { get; set; }
        public DbSet<RiskCalculationContentEntity> RiskCalculationContent { get; set; }
        public DbSet<ResourceBundleContentEntity> ResourceBundleContent { get; set; }
        public DbSet<AppConfigContentEntity> AppConfigContent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfiguration(new Configuration.Content.EksCreateJobInput());
            modelBuilder.ApplyConfiguration(new Configuration.Content.EksCreateJobOutput());
            modelBuilder.ApplyConfiguration(new Configuration.Content.ExposureKeySetContent());
            modelBuilder.ApplyConfiguration(new Configuration.Content.Manifest());
            modelBuilder.ApplyConfiguration(new Configuration.Content.RiskCalculationContent());
            modelBuilder.ApplyConfiguration(new Configuration.Content.ResourceBundleContent());
            modelBuilder.ApplyConfiguration(new Configuration.Content.AppConfigContent());
        }
    }
}
