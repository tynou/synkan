using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class ColumnRepository(AppDbContext context) : IColumnRepository
{
    public async Task Add(Column column)
    {
        context.Columns.Add(column);
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
}