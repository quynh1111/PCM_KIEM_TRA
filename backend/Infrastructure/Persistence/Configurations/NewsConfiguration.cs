using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCM.Domain.Entities;

namespace PCM.Infrastructure.Persistence.Configurations
{
    public class NewsConfiguration : IEntityTypeConfiguration<News>
    {
        public void Configure(EntityTypeBuilder<News> builder)
        {
            builder.ToTable("020_News");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(n => n.Content)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(n => n.IsPinned)
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedBy)
                .HasMaxLength(450);

            builder.HasIndex(n => n.IsPinned);
        }
    }
}
