using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class TournamentMatchConfiguration : IEntityTypeConfiguration<TournamentMatch>
    {
        public void Configure(EntityTypeBuilder<TournamentMatch> builder)
        {
            builder.ToTable("020_TournamentMatches");

            builder.HasKey(tm => tm.Id);

            builder.Property(tm => tm.BracketGroup)
                .HasMaxLength(50);

            builder.HasOne(tm => tm.Tournament)
                .WithMany(t => t.TournamentMatches)
                .HasForeignKey(tm => tm.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(tm => tm.Match)
                .WithMany()
                .HasForeignKey(tm => tm.MatchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(tm => new { tm.TournamentId, tm.Round, tm.Position });
        }
    }
}
