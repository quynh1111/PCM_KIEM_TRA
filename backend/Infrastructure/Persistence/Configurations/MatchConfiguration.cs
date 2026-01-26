using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class MatchConfiguration : IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.ToTable("020_Matches");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.MatchFormat)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(m => m.Result)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(m => m.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne(m => m.Tournament)
                .WithMany(t => t.Matches)
                .HasForeignKey(m => m.TournamentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(m => m.TournamentId);
        }
    }
}
