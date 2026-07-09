using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IBoardMemberRepository
{
    Task<BoardMember?> GetAsync(Guid boardId, Guid userId);
}