using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface IBoardRepository
{
    Task<Guid> Create(Board board);

    Task<Board?> GetById(Guid id);
}