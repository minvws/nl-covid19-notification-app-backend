// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
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

        public DbSet<ManifestEntity> ManifestContent { get; set; }
        public DbSet<ExposureKeySetContentEntity> ExposureKeySetContent { get; set; }
        public DbSet<RiskCalculationContentEntity> RiskCalculationContent { get; set; }
        public DbSet<ResourceBundleContentEntity> ResourceBundleContent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ExposureKeySetContent());
            modelBuilder.ApplyConfiguration(new Configuration.Content.Manifest());
            modelBuilder.ApplyConfiguration(new RiskCalculationContent());
            modelBuilder.ApplyConfiguration(new ResourceBundleContent());
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    //modelBuilder.Entity<ManifestEntity>(x =>
        //    //{
        //    //    x.HasNoDiscriminator();
        //    //});
        //    //modelBuilder.Entity<ExposureKeySetContentEntity>(x =>
        //    //{
        //    //    x.HasNoDiscriminator();
        //    //});
        //    //modelBuilder.Entity<RiskCalculationContentEntity>(x =>
        //    //{
        //    //    x.HasNoDiscriminator();
        //    //});
        //    //modelBuilder.Entity<ResourceBundleContentEntity>(x =>
        //    //{
        //    //    x.HasNoDiscriminator();
        //    //});
        //}

    }
}
