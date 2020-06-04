// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RivmAdvice;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase
{
    public class ExposureContentDbContext : DbContext
    {
        public ExposureContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public Microsoft.EntityFrameworkCore.DbSet<ManifestEntity> ManifestContent { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ExposureKeySetContentEntity> ExposureKeySetContent { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<RiskCalculationContentEntity> RiskCalculationulationContent { get; set; }
        public Microsoft.EntityFrameworkCore.DbSet<ResourceBundleContentEntity> ResourceBundleContent { get; set; }

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
