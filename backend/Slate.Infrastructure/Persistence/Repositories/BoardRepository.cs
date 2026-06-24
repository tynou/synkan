using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class BoardRepository(AppDbContext context) : IBoardRepository
{
    public async Task Create(Board board)
    {
        context.Boards.Add(board);
        await context.SaveChangesAsync();
    }

    public async Task<Board?> GetById(Guid boardId)
    {
        return await context.Boards
            .AsNoTracking()
            .Include(b => b.Members)
            .Include(b => b.Columns)
                .ThenInclude(c => c.Cards)
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }

    public async Task<List<Board>> GetBoardsByUserId(Guid userId)
    {
        return await context.Boards
            .AsNoTracking()
            .Where(b => b.Members.Any(m => m.Id == userId))
            .ToListAsync();
    }
}