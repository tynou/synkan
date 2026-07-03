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

    public async Task Delete(Guid boardId)
    {
        context.Boards.Remove(context.Boards.First(b => b.Id == boardId));
        await context.SaveChangesAsync();
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

    public async Task<bool> UserHasReadAccess(Guid boardId, Guid userId)
    {
        return await context.Boards
            .AsNoTracking()
            .Where(b => b.Id == boardId)
            .AnyAsync(b => b.IsPublic
                           || b.OwnerId == userId
                           || b.Members.Any(m => m.UserId == userId));
    }

    public async Task SaveChangesAsync() => await context.SaveChangesAsync();
}