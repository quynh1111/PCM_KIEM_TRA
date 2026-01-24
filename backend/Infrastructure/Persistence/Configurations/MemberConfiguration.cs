using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            // IMPORTANT: Replace XXX with last 3 digits of your student ID
            builder.ToTable("020_Members");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasIndex(m => m.UserId)
                .IsUnique();

            builder.Property(m => m.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(m => m.Email)
                .IsUnique();

            builder.Property(m => m.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(m => m.RankELO)
                .HasDefaultValue(1200.0);

            builder.Property(m => m.WalletBalance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0m);

            builder.Property(m => m.IsActive)
                .HasDefaultValue(true);

            // Concurrency token
            builder.Property(m => m.RowVersion)
                .IsRowVersion();

            // Relationships
            builder.HasMany(m => m.Bookings)
                .WithOne(b => b.Member)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(m => m.WalletTransactions)
                .WithOne(w => w.Member)
                .HasForeignKey(w => w.MemberId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
