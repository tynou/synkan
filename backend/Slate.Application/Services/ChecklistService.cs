using Microsoft.AspNetCore.SignalR;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;
using Slate.Domain.Exceptions;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class ChecklistService(
    ICardRepository cardRepository, 
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IChecklistService
{
    public async Task<Guid> Create(Guid userId, Guid cardId, string title)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        var checklist = card.AddChecklist(title);
        
        await cardRepository.SaveChangesAsync();
        
        // await hubContext.Clients
        //     .Group(card.BoardId.ToString())
        //     .OnChecklistCreated(cardId, title);

        return checklist.Id;
    }

    public async Task<Guid> CreateItem(Guid userId, Guid cardId, Guid checklistId, string text)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        var item = card.AddChecklistItem(checklistId, text);
        
        await cardRepository.SaveChangesAsync();

        return item.Id;
    }

    public async Task ToggleItem(Guid userId, Guid cardId, Guid checklistId, Guid itemId, bool isCompleted)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.ToggleChecklistItem(checklistId, itemId, isCompleted);
        
        await cardRepository.SaveChangesAsync();
    }

    public async Task Delete(Guid userId, Guid cardId, Guid checklistId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.RemoveChecklist(checklistId);
        
        await cardRepository.SaveChangesAsync();
    }

    public async Task DeleteItem(Guid userId, Guid cardId, Guid checklistId, Guid itemId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.RemoveChecklistItem(checklistId, itemId);
        
        await cardRepository.SaveChangesAsync();
    }
}