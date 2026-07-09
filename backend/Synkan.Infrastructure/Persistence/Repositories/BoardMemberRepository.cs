using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class BoardMemberRepository(AppDbContext context) : IBoardMemberRepository
{
    public async Task<BoardMember?> GetAsync(Guid boardId, Guid userId)
    {
        return await context.BoardMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(bm => bm.BoardId == boardId && bm.UserId == userId);
    }
}