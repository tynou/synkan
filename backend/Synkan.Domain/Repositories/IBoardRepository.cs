using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IBoardRepository
{
    Task Create(Board board);

    Task Delete(Guid boardId);

    Task<Board?> GetById(Guid boardId);
    
    Task<List<Board>> GetBoardsByUserId(Guid userId);
    
    Task<bool> UserHasReadAccess(Guid boardId, Guid userId);

    Task SaveChangesAsync();
}