namespace Slate.Application.Interfaces;

public interface IChecklistService
{
    Task<Guid> Create(Guid userId, Guid cardId, string title);
    
    Task CreateItem(Guid userId, Guid cardId, Guid checklistId, string text);

    Task ToggleItem(Guid userId, Guid cardId, Guid checklistId, Guid itemId, bool isCompleted);
    
    Task Delete(Guid userId, Guid cardId, Guid checklistId);
    
    Task DeleteItem(Guid userId, Guid cardId, Guid checklistId, Guid itemId);
}