// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Workflow
{
    public class TemporaryExposureKeyConfig : IEntityTypeConfiguration<TemporaryExposureKeyEntity>
    {
        public void Configure(EntityTypeBuilder<TemporaryExposureKeyEntity> builder)
        {
            builder.Property(u => u.Id).UseHiLo();
        }
    }
}
