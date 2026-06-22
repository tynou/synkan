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

    public async Task<Board?> GetById(Guid id)
    {
        return await context.Boards.Where(b => b.Id == id).FirstOrDefaultAsync();
    }
}