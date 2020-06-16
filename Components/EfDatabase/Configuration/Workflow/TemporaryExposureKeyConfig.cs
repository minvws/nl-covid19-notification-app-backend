using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NL.Rijksoverheid.ExposureNotification.BackEnd.Components.Workflows.KeysLastWorkflow;

namespace NL.Rijksoverheid.ExposureNotification.BackEnd.Components.EfDatabase.Configuration.Workflow
{
    public class TemporaryExposureKeyConfig : IEntityTypeConfiguration<TemporaryExposureKeyEntity>
    {
        public void Configure(EntityTypeBuilder<TemporaryExposureKeyEntity> builder)
        {
            builder.Property(u => u.Id).HasDefaultValueSql("NEWID()");
        }
    }
}
