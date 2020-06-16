// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class ICCBackendContentDbContext : DbContext
    {
        public ICCBackendContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<InfectionConfirmationCodeEntity> InfectionConfirmationCodes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InfectionConfirmationCodeEntity>().HasKey(e => e.Code);
            
            modelBuilder.Entity<InfectionConfirmationCodeEntity>().Property(e => e.Created)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InfectionConfirmationCodeEntity>().Property(e => e.GeneratedBy)
                .IsRequired();
        }
    }
}