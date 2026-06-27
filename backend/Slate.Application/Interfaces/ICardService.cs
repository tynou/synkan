using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface ICardService
{
    Task<Guid> Create(Guid userId, Guid boardId, Guid columnId, string title);
    
    Task Update(Guid userId, Guid cardId, string newTitle, string newDescription, Guid newColumnId, int newPosition);
    
    Task Delete(Guid userId, Guid cardId);
    
    Task<CardDto?> GetById(Guid cardId);
}