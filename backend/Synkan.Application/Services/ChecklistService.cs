using Microsoft.AspNetCore.SignalR;
using Synkan.Application.Events;
using Synkan.Application.Hubs;
using Synkan.Application.Interfaces;
using Synkan.Application.Mappers;
using Synkan.Domain.Exceptions;
using Synkan.Domain.Repositories;

namespace Synkan.Application.Services;

public class ChecklistService(
    ICurrentUserService currentUser,
    ICardRepository cardRepository,
    IUnitOfWork unitOfWork,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IChecklistService
{
    private Guid UserId => currentUser.UserId;
    
    public async Task<Guid> Create(Guid cardId, string title)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        var checklist = card.AddChecklist(title);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnChecklistCreated(checklist.ToDto());

        return checklist.Id;
    }

    public async Task<Guid> CreateItem(Guid cardId, Guid checklistId, string text)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        var item = card.AddChecklistItem(checklistId, text);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnChecklistItemCreated(item.ToDto());

        return item.Id;
    }

    public async Task ToggleItem(Guid cardId, Guid checklistId, Guid itemId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.ToggleChecklistItem(checklistId, itemId);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnChecklistItemToggled(new ChecklistItemToggledEvent(cardId, checklistId, itemId));
    }

    public async Task Delete(Guid cardId, Guid checklistId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.RemoveChecklist(checklistId);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnChecklistDeleted(new ChecklistDeletedEvent(cardId, checklistId));
    }

    public async Task DeleteItem(Guid cardId, Guid checklistId, Guid itemId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.RemoveChecklistItem(checklistId, itemId);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnChecklistItemDeleted(new ChecklistItemDeletedEvent(cardId, checklistId, itemId));
    }
}