using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class BoardRepository(AppDbContext context) : IBoardRepository
{
    public async Task Create(Board board)
    {
        context.Boards.Add(board);
    }

    public async Task Delete(Guid boardId)
    {
        var board = context.Boards.FirstOrDefault(b => b.Id == boardId);
        if (board is null)
        {
            return;
        }
        context.Boards.Remove(board);
    }

    public async Task<Board?> GetById(Guid boardId)
    {
        return await context.Boards
            .Include(b => b.Members)
                .ThenInclude(m => m.User)
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Position))
                    .ThenInclude(c => c.Checklists)
                        .ThenInclude(cl => cl.Items.OrderBy(i => i.Position))
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Position))
                    .ThenInclude(c => c.Labels)
            .Include(b => b.AvailableLabels)
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }

    public async Task<List<Board>> GetBoardsByUserId(Guid userId)
    {
        return await context.Boards
            .AsNoTracking()
            .Where(b => b.Members.Any(m => m.UserId == userId))
            .Include(b => b.Members)
                .ThenInclude(m => m.User)
            .Include(b => b.Columns.OrderBy(c => c.Position))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Position))
            .ToListAsync();
    }
}