using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.ToTable("020_WalletTransactions");

            builder.HasKey(w => w.Id);

            builder.Property(w => w.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(w => w.Type)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(w => w.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(w => w.Description)
                .HasMaxLength(500);

            builder.Property(w => w.ReferenceId)
                .HasMaxLength(100);

            builder.Property(w => w.EncryptedSignature)
                .HasMaxLength(256);

            builder.Property(w => w.ProofImageUrl)
                .HasMaxLength(500);

            // Relationships
            builder.HasOne(w => w.Member)
                .WithMany(m => m.WalletTransactions)
                .HasForeignKey(w => w.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(w => w.Category)
                .WithMany()
                .HasForeignKey(w => w.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(w => w.MemberId);
            builder.HasIndex(w => w.Status);
            builder.HasIndex(w => w.Type);
            builder.HasIndex(w => w.Date);
        }
    }
}
