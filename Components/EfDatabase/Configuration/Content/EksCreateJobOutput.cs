using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.ExposureKeySets;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Content
{
    public class EksCreateJobOutput : IEntityTypeConfiguration<EksCreateJobOutputEntity>
    {
        public void Configure(EntityTypeBuilder<EksCreateJobOutputEntity> builder)
        {
            builder.ToTable("EksCreateJobOutput");
            builder.Property(u => u.Id).UseHiLo();
        }
    }
}