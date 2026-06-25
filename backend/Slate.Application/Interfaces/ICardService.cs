using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface ICardService
{
    Task<Guid> Create(Guid userId, Guid boardId, Guid columnId, string title);
    
    Task Delete(Guid userId, Guid cardId);
    
    Task<CardDto?> GetById(Guid cardId);
}