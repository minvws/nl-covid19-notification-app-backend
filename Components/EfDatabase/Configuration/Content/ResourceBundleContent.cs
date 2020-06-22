// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ResourceBundle;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class ResourceBundleContent : IEntityTypeConfiguration<ResourceBundleContentEntity>
    {
        public void Configure(EntityTypeBuilder<ResourceBundleContentEntity> builder)
        {
            builder.ToTable("ResourceBundleContent");
            builder.Property(u => u.Id).UseHiLo();
            builder.HasIndex(x => x.PublishingId).IsUnique();
            builder.Property(u => u.PublishingId).HasMaxLength(64);
        }
    }
}
