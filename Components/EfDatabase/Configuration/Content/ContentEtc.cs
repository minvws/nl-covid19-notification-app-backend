// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Content;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class ContentEtc : IEntityTypeConfiguration<ContentEntity>
    {
        public void Configure(EntityTypeBuilder<ContentEntity> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ToTable("Content");
            builder.Property(u => u.PublishingId).HasMaxLength(64);

            builder.HasIndex(u => u.PublishingId);
            builder.HasIndex(u => u.Type);
            builder.HasIndex(u => u.Release);
            builder.HasIndex(u => u.ContentTypeName);

            builder.Property(u => u.Id).UseHiLo();
        }
    }
}
