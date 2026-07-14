namespace Synkan.Application.Interfaces;

public interface IAiToolsService
{
    Task<string> CreateColumn(string boardId, string title);
    
    Task<string> UpdateColumnTitle(string columnId, string newTitle);
    
    Task<string> MoveColumn(string columnId, int newPosition);
    
    Task<string> DeleteColumn(string columnId);
    
    
    Task<string> CreateCard(string columnId, string title);
    
    Task<string> UpdateCardContent(string cardId, string newTitle, string newDescription);
    
    Task<string> UpdateCardCover(string cardId, string color);
    
    Task<string> MoveCard(string cardId, string newColumnId, int newPosition);
    
    Task<string> DeleteCard(string cardId);
    
    
    Task<string> AssignCardLabel(string cardId, string labelId);
    
    Task<string> RemoveCardLabel(string cardId, string labelId);
    
    Task<string> CreateLabel(string boardId, string name, string color);
    
    
    Task<string> CreateChecklist(string cardId, string title);
    
    Task<string> DeleteChecklist(string cardId, string checklistId);
    
    Task<string> CreateChecklistItem(string cardId, string checklistId, string itemText);
    
    Task<string> DeleteChecklistItem(string cardId, string checklistId, string itemId);
    
    Task<string> ToggleChecklistItem(string cardId, string checklistId, string itemId);
}