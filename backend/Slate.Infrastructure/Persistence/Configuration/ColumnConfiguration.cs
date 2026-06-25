using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slate.Domain.Entities;

namespace Slate.Infrastructure.Persistence.Configuration;

public class ColumnConfiguration : IEntityTypeConfiguration<Column>
{
    public void Configure(EntityTypeBuilder<Column> builder)
    {
        builder.HasKey(c => c.Id);

        builder.HasMany(c => c.Cards)
            .WithOne(c => c.Column)
            .HasForeignKey(c => c.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}