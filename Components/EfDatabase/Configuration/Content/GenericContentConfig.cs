// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ContentLoading;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class GenericContentConfig : IEntityTypeConfiguration<GenericContentEntity>
    {
        public void Configure(EntityTypeBuilder<GenericContentEntity> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ToTable("GenericContent");
            builder.Property(u => u.PublishingId).HasMaxLength(64);

            builder.HasIndex(u => u.PublishingId);
            builder.HasIndex(u => u.GenericType);
            builder.HasIndex(u => u.Release);
            builder.HasIndex(u => u.SignedContentTypeName);

            builder.Property(u => u.Id).UseHiLo();
        }
    }
}
