using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface IBoardMemberRepository
{
    Task<BoardMember?> GetAsync(Guid boardId, Guid userId);
}