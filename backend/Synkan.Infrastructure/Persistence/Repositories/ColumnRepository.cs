using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

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
            .Include(c => c.Cards.OrderBy(card => card.Position))
                .ThenInclude(c => c.Checklists)
                    .ThenInclude(cl => cl.Items.OrderBy(i => i.Position))
            .Include(c => c.Cards.OrderBy(card => card.Position))
                .ThenInclude(c => c.Labels)
            .Include(c => c.Board)
                .ThenInclude(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == columnId);
    }
    
    public async Task<Column?> GetByIdWithBoard(Guid columnId)
    {
        return await context.Columns
            .Include(c => c.Cards.OrderBy(card => card.Position))
            .Include(c => c.Board)
                .ThenInclude(b => b.Columns)
            .Include(c => c.Board)
                .ThenInclude(b => b.Members)
            .FirstOrDefaultAsync(b => b.Id == columnId);
    }
    
    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}