using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("020_Bookings");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.TotalPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(b => b.Status)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(b => b.Note)
                .HasMaxLength(500);

            // CRITICAL: Concurrency token for optimistic locking
            builder.Property(b => b.RowVersion)
                .IsRowVersion();

            // Relationships
            builder.HasOne(b => b.Court)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CourtId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Member)
                .WithMany(m => m.Bookings)
                .HasForeignKey(b => b.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            builder.HasIndex(b => new { b.CourtId, b.StartTime, b.EndTime });
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.RecurringGroupId);
        }
    }
}
