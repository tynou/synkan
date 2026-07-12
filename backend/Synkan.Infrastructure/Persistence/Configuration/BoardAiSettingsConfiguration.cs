using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Synkan.Domain.Entities;

namespace Synkan.Infrastructure.Persistence.Configuration;

public class BoardAiSettingsConfiguration : IEntityTypeConfiguration<BoardAiSettings>
{
    public void Configure(EntityTypeBuilder<BoardAiSettings> builder)
    {
        builder.HasKey(s => s.BoardId);
        
        builder.HasOne<Board>()
            .WithOne()
            .HasForeignKey<BoardAiSettings>(s => s.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}