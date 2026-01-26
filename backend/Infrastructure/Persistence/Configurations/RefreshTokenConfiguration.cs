using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("020_RefreshTokens");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Property(r => r.Token)
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(r => r.JwtId)
                .HasMaxLength(450);

            builder.HasIndex(r => r.UserId);
            builder.HasIndex(r => r.Token).IsUnique();
        }
    }
}
