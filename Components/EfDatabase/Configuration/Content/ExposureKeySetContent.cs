// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class ExposureKeySetContent : IEntityTypeConfiguration<ExposureKeySetContentEntity>
    {
        public void Configure(EntityTypeBuilder<ExposureKeySetContentEntity> builder)
        {
            builder.ToTable("ExposureKeySetContent");
        }
    }
    public class EksCreateJobInput : IEntityTypeConfiguration<EksCreateJobInputEntity>
    {
        public void Configure(EntityTypeBuilder<EksCreateJobInputEntity> builder)
        {
            builder.ToTable("EksCreateJobInput");
        }
    }

    public class EksCreateJobOutput : IEntityTypeConfiguration<EksCreateJobOutputEntity>
    {
        public void Configure(EntityTypeBuilder<EksCreateJobOutputEntity> builder)
        {
            builder.ToTable("EksCreateJobOutput");
        }
    }
}
