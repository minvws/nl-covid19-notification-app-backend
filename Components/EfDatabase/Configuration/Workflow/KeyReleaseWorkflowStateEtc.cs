// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Workflow
{
    public class KeyReleaseWorkflowStateEtc : IEntityTypeConfiguration<KeyReleaseWorkflowState>
    {
        public void Configure(EntityTypeBuilder<KeyReleaseWorkflowState> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            builder.ToTable("KeyReleaseWorkflowState");
            builder.Property(u => u.Id).UseIdentityColumn();

            builder.HasIndex(x => x.BucketId) /*.IsUnique()*/; //TODO genteks... 
            builder.HasIndex(x => x.ConfirmationKey)/*.IsUnique()*/; //TODO genteks... 
            builder.HasIndex(x => x.LabConfirmationId)/*.IsUnique()*/; //TODO genteks... 
            builder.HasIndex(x => x.PollToken)/*.IsUnique()*/; //TODO genteks... 

            builder.HasIndex(x => x.ValidUntil);
            builder.HasIndex(x => x.Authorised);
            builder.HasIndex(x => x.AuthorisedByCaregiver);

            builder
                .HasMany(x => x.Keys)
                .WithOne(x => x.Owner)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
