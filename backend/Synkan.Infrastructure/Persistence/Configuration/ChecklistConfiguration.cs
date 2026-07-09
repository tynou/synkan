using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Synkan.Domain.Entities;

namespace Synkan.Infrastructure.Persistence.Configuration;

public class ChecklistConfiguration : IEntityTypeConfiguration<Checklist>
{
    public void Configure(EntityTypeBuilder<Checklist> builder)
    {
        builder.HasKey(cl => cl.Id);
        
        builder.Property(cl => cl.Title)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.HasMany(cl => cl.Items)
            .WithOne()
            .HasForeignKey(item => item.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}