using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class BoardMemberRepository(AppDbContext context) : IBoardMemberRepository
{
    public async Task<BoardMember?> GetAsync(Guid boardId, Guid userId)
    {
        return await context.BoardMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
    }
}