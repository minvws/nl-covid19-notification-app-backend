// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.RiskCalculationConfig;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class RiskCalculationContent : IEntityTypeConfiguration<RiskCalculationContentEntity>
    {
        public void Configure(EntityTypeBuilder<RiskCalculationContentEntity> builder)
        {
            builder.ToTable("RiskCalculationContent");
            builder.Property(u => u.Id).UseHiLo();
            builder.HasIndex(x => x.PublishingId).IsUnique();
            builder.Property(u => u.PublishingId).HasMaxLength(64);
        }
    }
}
