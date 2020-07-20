// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.GenericContent;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Contexts
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ManifestEntity> ManifestContent { get; set; }
        public DbSet<ExposureKeySetContentEntity> ExposureKeySetContent { get; set; }
        public DbSet<GenericContentEntity> GenericContent { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.HasDefaultSchema("dbo");
            modelBuilder.ApplyConfiguration(new Configuration.Content.ExposureKeySetContentEtc());
            modelBuilder.ApplyConfiguration(new Configuration.Content.ManifestEtc());
            modelBuilder.ApplyConfiguration(new Configuration.Content.GenericContentEtc());
        }
    }
}
