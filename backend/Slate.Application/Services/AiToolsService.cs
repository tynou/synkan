using System.ComponentModel;
using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class AiToolsService(
    ICardService cardService,
    IColumnService columnService,
    IChecklistService checklistService,
    ICurrentUserService currentUser
    ) : IAiToolsService
{
    [Description("Creates a new column")] 
    public async Task<string> CreateColumn(
        [Description("The Id of the board")] string boardId,
        [Description("The title of the new column")] string title
    )
    {
        var result = await columnService.Create(currentUser.UserId, Guid.Parse(boardId), title);
        return $"Column created: {result.ToString()}";
    }

    [Description("Changes the title of the column")] 
    public async Task<string> UpdateColumnTitle(
        [Description("The Id of the column")] string columnId,
        [Description("The new title")] string newTitle
    )
    {
        await columnService.UpdateTitle(currentUser.UserId, Guid.Parse(columnId), newTitle);
        return "Column title changed";
    }

    [Description("Moves the column")] 
    public async Task<string> MoveColumn(
        [Description("The Id of the column")] string columnId,
        [Description("The new position")] int newPosition
    )
    {
        await columnService.Move(currentUser.UserId, Guid.Parse(columnId), newPosition);
        return "Column moved";
    }

    [Description("Deletes the column")] 
    public async Task<string> DeleteColumn(
        [Description("The Id of the column")] string columnId
    )
    {
        await columnService.Delete(currentUser.UserId, Guid.Parse(columnId));
        return "Column deleted";
    }

    [Description("Creates a new card in the specified column of the board")]
    public async Task<string> CreateCard(
        [Description("The Id of the column")] string columnId,
        [Description("The title of the card")] string title
    )
    {
        Console.WriteLine($"Does this work? {columnId} {title}");
        var result = await cardService.Create(currentUser.UserId, Guid.Parse(columnId), title);
        return $"Card created: {result.ToString()}";
    }

    [Description("Updates the title and description of the card")]
    public async Task<string> UpdateCardContent(
        [Description("The Id of the card")] string cardId,
        [Description("The new title of the card")] string newTitle,
        [Description("The new description of the card")] string newDescription
    )
    {
        await cardService.UpdateContent(currentUser.UserId, Guid.Parse(cardId), newTitle, newDescription);
        return "Card content changed";
    }

    [Description("Changes the color of the card")] 
    public async Task<string> UpdateCardCover(
        [Description("The Id of the card")] string cardId,
        [Description("The color in hex string format")] string color
    )
    {
        await cardService.UpdateCover(currentUser.UserId, Guid.Parse(cardId), color);
        return "Card color changed";
    }

    [Description("Moves the card to the specified position in the specified column")] 
    public async Task<string> MoveCard(
        [Description("The Id of the card")] string cardId, 
        [Description("The Id of the new column")] string newColumnId, 
        [Description("The new position")] int newPosition
    )
    {
        await cardService.Move(currentUser.UserId, Guid.Parse(cardId), Guid.Parse(newColumnId), newPosition);
        return "Card moved";
    }

    [Description("Deletes the card")] 
    public async Task<string> DeleteCard(
        [Description("The Id of the card")] string cardId
    )
    {
        await cardService.Delete(currentUser.UserId, Guid.Parse(cardId));
        return "Card deleted";
    }

    [Description("Creates a checklist in the specified card with the given title")] 
    public async Task<string> CreateChecklist(
        [Description("The Id of the card")] string cardId,
        [Description("The title of the checklist")] string title
    )
    {
        var result = await checklistService.Create(currentUser.UserId, Guid.Parse(cardId), title);
        return $"Checklist created: {result.ToString()}";
    }

    [Description("Deletes the checklist from the specified card")] 
    public async Task<string> DeleteChecklist(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId
    )
    {
        await checklistService.Delete(currentUser.UserId, Guid.Parse(cardId), Guid.Parse(checklistId));
        return "Checklist deleted";
    }

    [Description("Creates a new checklist item")] 
    public async Task<string> CreateChecklistItem(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId,
        [Description("The text of the item")] string itemText
    )
    {
        var result = await checklistService.CreateItem(currentUser.UserId, Guid.Parse(cardId), Guid.Parse(checklistId), itemText);
        return $"Checklist item created: {result.ToString()}";
    }

    [Description("Deletes the checklist item")] 
    public async Task<string> DeleteChecklistItem(
        [Description("The Id of the card")] string cardId,
        [Description("The Id of the checklist")] string checklistId,
        [Description("The Id of the checklist item")] string itemId
    )
    {
        await checklistService.DeleteItem(currentUser.UserId, Guid.Parse(cardId), Guid.Parse(checklistId), Guid.Parse(itemId));
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
        await checklistService.ToggleItem(
            currentUser.UserId,
            Guid.Parse(cardId),
            Guid.Parse(checklistId),
            Guid.Parse(itemId),
            isCompleted
        );
        return "Checklist item toggled";
    }
}