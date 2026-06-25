using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slate.Domain.Entities;

namespace Slate.Infrastructure.Persistence.Configuration;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(1000);
    }
}