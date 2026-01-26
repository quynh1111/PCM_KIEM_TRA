using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
    {
        public void Configure(EntityTypeBuilder<Participant> builder)
        {
            builder.ToTable("020_Participants");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.TeamName)
                .HasMaxLength(200);

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.HasIndex(p => new { p.TournamentId, p.MemberId }).IsUnique();

            builder.HasOne(p => p.Tournament)
                .WithMany(t => t.Participants)
                .HasForeignKey(p => p.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Member)
                .WithMany(m => m.Participants)
                .HasForeignKey(p => p.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
