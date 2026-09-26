using ClientEngagementFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientEngagementFlow.Infastructure.Persistence.Configurations
{
    public class ProcessingJobConfiguration : IEntityTypeConfiguration<ProcessingJob>
    {
        public void Configure(EntityTypeBuilder<ProcessingJob> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .IsRequired();

            builder.Property(x => x.CreatedUtc)
                .IsRequired();

            builder.Property(x => x.FailureReason)
                .HasMaxLength(1000);
        }
    }
}
