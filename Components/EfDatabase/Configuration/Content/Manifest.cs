// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Manifest;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class Manifest : IEntityTypeConfiguration<ManifestEntity>
    {
        public void Configure(EntityTypeBuilder<ManifestEntity> builder)
        {
            builder.ToTable("Manifest");
            builder.Property(u => u.Id).UseHiLo();
            builder.HasIndex(x => x.PublishingId).IsUnique();
            builder.Property(u => u.PublishingId).HasMaxLength(64);
        }
    }
}
