namespace Synkan.Application.Interfaces;

public interface IChecklistService
{
    Task<Guid> Create(Guid cardId, string title);
    
    Task<Guid> CreateItem(Guid cardId, Guid checklistId, string text);

    Task ToggleItem(Guid cardId, Guid checklistId, Guid itemId, bool isCompleted);
    
    Task Delete(Guid cardId, Guid checklistId);
    
    Task DeleteItem(Guid cardId, Guid checklistId, Guid itemId);
}