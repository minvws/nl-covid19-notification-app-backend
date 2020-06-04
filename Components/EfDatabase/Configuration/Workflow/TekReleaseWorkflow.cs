// Copyright © 2020 De Staat der Nederlanden, Ministerie van Volksgezondheid, Welzijn en Sport.
// Licensed under the EUROPEAN UNION PUBLIC LICENCE v. 1.2
// SPDX-License-Identifier: EUPL-1.2

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Workflow
{
    public class TekReleaseWorkflow : IEntityTypeConfiguration<TekReleaseWorkflowEntity>
    {
        public void Configure(EntityTypeBuilder<TekReleaseWorkflowEntity> builder)
        {
            builder.ToTable("TekReleaseWorkflowEntity");
        }
    }
}
