using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class ColumnRepository(AppDbContext context) : IColumnRepository
{
    public async Task Create(Column column)
    {
        context.Columns.Add(column);
        await context.SaveChangesAsync();
    }

    public async Task Delete(Guid columnId)
    {
        context.Columns.Remove(context.Columns.First(c => c.Id == columnId));
        await context.SaveChangesAsync();
    }

    public async Task<Column?> GetById(Guid columnId)
    {
        return await context.Columns
            .Include(c => c.Cards)
            .FirstOrDefaultAsync(b => b.Id == columnId);
    }
    
    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}