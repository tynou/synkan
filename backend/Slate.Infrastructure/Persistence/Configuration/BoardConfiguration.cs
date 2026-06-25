using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slate.Domain.Entities;

namespace Slate.Infrastructure.Persistence.Configuration;

public class BoardConfiguration : IEntityTypeConfiguration<Board>
{
    public void Configure(EntityTypeBuilder<Board> builder)
    {
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Title)
            .HasMaxLength(150)
            .IsRequired();
        
        builder.HasMany(b => b.Members)
            .WithMany()
            .UsingEntity(j => j.ToTable("BoardMembers"));
    }
}