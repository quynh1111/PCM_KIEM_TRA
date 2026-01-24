using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class TreasuryTransactionConfiguration : IEntityTypeConfiguration<TreasuryTransaction>
    {
        public void Configure(EntityTypeBuilder<TreasuryTransaction> builder)
        {
            builder.ToTable("020_TreasuryTransactions"); // TODO: replace XXX_ with your MSSV prefix
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);

            builder.HasOne(x => x.Category)
                   .WithMany()
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.CreatedByMemberId).IsRequired();
            builder.Property(x => x.CreatedDate).IsRequired();
        }
    }
}
