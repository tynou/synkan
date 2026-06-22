using Slate.Domain.Entities;

namespace Slate.Application.Interfaces;

public interface IBoardService
{
    Task<Guid> Create(Guid userId, string title);
    
    Task<Board?> GetById(Guid id);
}