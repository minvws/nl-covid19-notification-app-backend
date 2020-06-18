// Copyright 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Job
{
    public class ExposureKeySet : IEntityTypeConfiguration<ExposureKeySetEntity>
    {
        public void Configure(EntityTypeBuilder<ExposureKeySetEntity> builder)
        {
            builder.ToTable("Output");
        }
    }
}
