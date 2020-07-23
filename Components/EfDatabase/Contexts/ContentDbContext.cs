// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ContentEntity> Content { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfiguration(new ContentEtc());
        }
    }
}
