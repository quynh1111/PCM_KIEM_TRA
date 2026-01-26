using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class CourtConfiguration : IEntityTypeConfiguration<Court>
    {
        public void Configure(EntityTypeBuilder<Court> builder)
        {
            builder.ToTable("020_Courts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .HasMaxLength(120)
                .IsRequired();

            builder.Property(c => c.Description)
                .HasMaxLength(500);

            builder.Property(c => c.HourlyRate)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.IsActive)
                .HasDefaultValue(true);
        }
    }
}
