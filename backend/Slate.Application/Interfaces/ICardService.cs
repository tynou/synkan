using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface ICardService
{
    Task<Guid> Create(Guid userId, Guid columnId, string title);
    
    Task UpdateContent(Guid userId, Guid cardId, string newTitle, string newDescription);
    
    Task UpdateCover(Guid userId, Guid cardId, string? color);
    
    Task UpdateDueDate(Guid cardId, DateTimeOffset dueDate, DateTimeOffset reminderTime);
    
    Task RemoveDueDate(Guid cardId);
    
    Task AssignLabel(Guid cardId, Guid labelId);
    
    Task RemoveLabel(Guid cardId, Guid labelId);
    
    Task Move(Guid userId, Guid cardId, Guid newColumnId, int newPosition);
    
    Task Delete(Guid userId, Guid cardId);
    
    Task<CardDto> GetById(Guid userId, Guid cardId);
}