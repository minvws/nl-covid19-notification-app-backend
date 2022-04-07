// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.Entities;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Content.Commands.EntityFramework
{
    public class ContentDbContext : DbContext
    {
        public ContentDbContext(DbContextOptions<ContentDbContext> options)
            : base(options)
        {
        }

        public DbSet<ContentEntity> Content { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Entity<ContentEntity>().HasIndex(u => u.PublishingId);
            modelBuilder.Entity<ContentEntity>().HasIndex(u => u.Type);
            modelBuilder.Entity<ContentEntity>().HasIndex(u => u.Release);
            modelBuilder.Entity<ContentEntity>().HasIndex(u => u.ContentTypeName);

            // Restrict the unique index on PublishingId and Type to ExposureKeySets
            var indexFilter =
                $"[PublishingId] IS NOT NULL AND [Type] IN ('{ContentTypes.ExposureKeySetV2}', '{ContentTypes.ExposureKeySetV3}')";

            modelBuilder
                .Entity<ContentEntity>()
                .HasIndex(e => new { e.PublishingId, e.Type })
                .IsUnique(true)
                .HasFilter(indexFilter);

            modelBuilder
                .Entity<ContentEntity>()
                .Property(e => e.Type)
                .HasConversion(
                    v => v.ToString(),
                    v => (ContentTypes)Enum.Parse(typeof(ContentTypes), v));
        }
    }
}
