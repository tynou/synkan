using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Slate.Domain.Entities;

namespace Slate.Infrastructure.Persistence.Configuration;

public class BoardMemberConfiguration : IEntityTypeConfiguration<BoardMember>
{
    public void Configure(EntityTypeBuilder<BoardMember> builder)
    {
        builder.HasKey(bm => new { bm.BoardId, bm.UserId });
        
        builder.Property(bm => bm.AccessLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
    }
}