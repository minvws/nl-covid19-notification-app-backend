// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Workflow
{
    public class KeyReleaseWorkflowStateConfig : IEntityTypeConfiguration<KeyReleaseWorkflowState>
    {
        public void Configure(EntityTypeBuilder<KeyReleaseWorkflowState> builder)
        {
            builder.ToTable("KeyReleaseWorkflowState");
            builder.Property(u => u.Id).UseHiLo();
            builder
                .HasMany(x => x.Keys)
                .WithOne(x => x.Owner)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
