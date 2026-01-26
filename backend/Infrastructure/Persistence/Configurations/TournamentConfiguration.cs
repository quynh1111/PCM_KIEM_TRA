using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
    {
        public void Configure(EntityTypeBuilder<Tournament> builder)
        {
            builder.ToTable("020_Tournaments");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(t => t.Description)
                .HasMaxLength(1000);

            builder.Property(t => t.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.Format)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(t => t.EntryFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.PrizePool)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.CreatedBy)
                .HasMaxLength(450);

            builder.HasIndex(t => t.Status);
        }
    }
}
