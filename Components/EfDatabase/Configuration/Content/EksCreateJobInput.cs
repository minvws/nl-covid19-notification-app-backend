using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySetsEngine;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class EksCreateJobInput : IEntityTypeConfiguration<EksCreateJobInputEntity>
    {
        public void Configure(EntityTypeBuilder<EksCreateJobInputEntity> builder)
        {
            builder.ToTable("EksCreateJobInput");
        }
    }
}