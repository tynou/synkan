using System.ComponentModel;
using Synkan.Application.Interfaces;

namespace Synkan.Application.Services;

public class AiToolsService(
    ICardService cardService,
    IColumnService columnService,
    IBoardService boardService,
    IChecklistService checklistService
    ) : IAiToolsService
{
    [Description("Creates a new column")] 
    public async Task<string> CreateColumn(
        [Description("The Id of the board")] string boardId,
        [Description("The title of the new column")] string title
    )
    {
        var result = await columnService.Create(Guid.Parse(boardId), title);
        return $"Column created: {result.ToString()}";
    }

    [Description("Changes the title of the column")] 
    public async Task<string> UpdateColumnTitle(
        [Description("The Id of the column")] string columnId,
        [Description("The new title")] string newTitle
    )
    {
        await columnService.UpdateTitle(Guid.Parse(columnId), newTitle);
        return "Column title changed";
    }

    [Description("Moves the column")] 
    public async Task<string> MoveColumn(
        [Description("The Id of the column")] string columnId,
        [Description("The new position")] int newPosition
    )
    {
        await columnService.Move(Guid.Parse(columnId), newPosition);
        return "Column moved";
    }

    [Description("Deletes the column")] 
    public async Task<string> DeleteColumn(
        [Description("The Id of the column")] string columnId
    )
    {
        await columnService.Delete(Guid.Parse(columnId));
        return "Column deleted";
    }

    [Description("Creates a new card in the specified column of the board")]
    public async Task<string> CreateCard(
        [Description("The Id of the column")] string columnId,
        [Description("The title of the card")] string title
    )
    {
        var result = await cardService.Create(Guid.Parse(columnId), title);
        return $"Card created: {result.ToString()}";
    }

    [Description("Updates the title and description of the card")]
    public async Task<string> UpdateCardContent(
        [Description("The Id of the card")] string cardId,
        [Description("The new title of the card")] string newTitle,
        [Description("The new description of the card")] string newDescription
    )
    {
        await cardService.UpdateContent(Guid.Parse(cardId), newTitle, newDescription);
        return "Card content changed";
    }

    [Description("Changes the color of the card")]
    public async Task<string> UpdateCardCover(
        [Description("The Id of the card")] string cardId,
        [Description("The color in hex string format")] string color
    )
    {
        await cardService.UpdateCover(Guid.Parse(cardId), color);
        return "Card color changed";
    }

    [Description("Moves the card to the specified position in the specified column")]
    public async Task<string> MoveCard(
        [Description("The Id of the card")] string cardId, 
        [Description("The Id of the new column")] string newColumnId, 
        [Description("The new position")] int newPosition
    )
    {
        await cardService.Move(Guid.Parse(cardId), Guid.Parse(newColumnId), newPosition);
        return "Card moved";
    }

    [Description("Deletes the card")]
    public async Task<string> DeleteCard(
        [Description("The Id of the card")] string cardId
    )
    {
        await cardService.Delete(Guid.Parse(cardId));
        return "Card deleted";
    }

    [Description("Assignes a label to the card")]
    public async Task<string> AssignCardLabel(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the label")] string labelId
    )
    {
        await cardService.AssignLabel(Guid.Parse(cardId), Guid.Parse(labelId));
        return "Label assigned";
    }

    [Description("Removes the label from the card")]
    public async Task<string> RemoveCardLabel(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the label")] string labelId
    )
    {
        await cardService.RemoveLabel(Guid.Parse(cardId), Guid.Parse(labelId));
        return "Label removed";
    }

    [Description("Creates a label that can be assigned to any cards on the board")]
    public async Task<string> CreateLabel(
        [Description("The Id of the board")] string boardId,
        [Description("The name of the label")] string name,
        [Description("The color of the label")] string color
    )
    {
        var result = await boardService.CreateLabel(Guid.Parse(boardId), name, color);
        return $"Label created: {result.ToString()}";
    }

    [Description("Creates a checklist in the specified card with the given title")] 
    public async Task<string> CreateChecklist(
        [Description("The Id of the card")] string cardId,
        [Description("The title of the checklist")] string title
    )
    {
        var result = await checklistService.Create(Guid.Parse(cardId), title);
        return $"Checklist created: {result.ToString()}";
    }

    [Description("Deletes the checklist from the specified card")] 
    public async Task<string> DeleteChecklist(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId
    )
    {
        await checklistService.Delete(Guid.Parse(cardId), Guid.Parse(checklistId));
        return "Checklist deleted";
    }

    [Description("Creates a new checklist item")] 
    public async Task<string> CreateChecklistItem(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId,
        [Description("The text of the item")] string itemText
    )
    {
        var result = await checklistService.CreateItem(Guid.Parse(cardId), Guid.Parse(checklistId), itemText);
        return $"Checklist item created: {result.ToString()}";
    }

    [Description("Deletes the checklist item")] 
    public async Task<string> DeleteChecklistItem(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId,
        [Description("The Id of the checklist item")] string itemId
    )
    {
        await checklistService.DeleteItem(Guid.Parse(cardId), Guid.Parse(checklistId), Guid.Parse(itemId));
        return "Checklist item deleted";
    }

    [Description("Toggles the checklist item")] 
    public async Task<string> ToggleChecklistItem(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId,
        [Description("The Id of the checklist item")] string itemId,
        [Description("The new state of the checklist item")] bool isCompleted
    )
    {
        await checklistService.ToggleItem(Guid.Parse(cardId), Guid.Parse(checklistId), Guid.Parse(itemId), isCompleted);
        return "Checklist item toggled";
    }
}