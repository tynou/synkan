using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface IBoardRepository
{
    Task Create(Board board);

    Task<Board?> GetById(Guid boardId);
    
    Task<List<Board>> GetBoardsByUserId(Guid userId);
}