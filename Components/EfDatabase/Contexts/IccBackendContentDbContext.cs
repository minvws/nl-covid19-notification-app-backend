// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Icc;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ICC.Models;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class IccBackendContentDbContext : DbContext
    {
        public IccBackendContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<InfectionConfirmationCodeEntity> InfectionConfirmationCodes { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<InfectionConfirmationCodeEntity>().HasKey(e => e.Code);
            modelBuilder.Entity<InfectionConfirmationCodeEntity>().HasIndex(e => e.BatchId);
            
            modelBuilder.Entity<InfectionConfirmationCodeEntity>().Property(e => e.Created)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<InfectionConfirmationCodeEntity>().Property(e => e.GeneratedBy)
                .IsRequired();
        }
    }
}