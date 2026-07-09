using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Synkan.Domain.Entities;

namespace Synkan.Infrastructure.Persistence.Configuration;

public class BoardMemberConfiguration : IEntityTypeConfiguration<BoardMember>
{
    public void Configure(EntityTypeBuilder<BoardMember> builder)
    {
        builder.HasKey(bm => new { bm.BoardId, bm.UserId });
        
        builder.HasOne(bm => bm.User)
            .WithMany()
            .HasForeignKey(bm => bm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(bm => bm.AccessLevel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
    }
}