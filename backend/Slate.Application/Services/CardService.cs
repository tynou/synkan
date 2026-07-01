using Microsoft.AspNetCore.SignalR;
using Slate.Application.Dto.Response;
using Slate.Application.Events;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Enums;
using Slate.Domain.Exceptions;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class CardService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    ICardRepository cardRepository,
    IBoardMemberRepository boardMemberRepository,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : ICardService
{
    public async Task<Guid> Create(Guid userId, Guid columnId, string title)
    {
        var column = await columnRepository.GetById(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");
        
        var member = await boardMemberRepository.GetAsync(column.BoardId, userId);
        if (member is null || member.AccessLevel == AccessLevel.Viewer)
            throw new UnauthorizedException("You do not have permission to modify this board.");
        
        var card = column.AddCard(title);
        
        await columnRepository.SaveChangesAsync();

        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardCreated(card.ToDto());

        return card.Id;
    }

    public async Task UpdateContent(Guid userId, Guid cardId, string newTitle, string newDescription)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found.");
        
        var member = await boardMemberRepository.GetAsync(card.BoardId, userId);
        if (member is null || member.AccessLevel == AccessLevel.Viewer)
            throw new UnauthorizedException("You do not have permission to modify this board.");
        
        card.SetTitle(newTitle);
        card.SetDescription(newDescription);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardContentUpdated(new CardContentUpdatedEvent(cardId, newTitle, newDescription));
    }
    
    public async Task Move(Guid userId, Guid cardId, Guid newColumnId, int newPosition)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found.");
        
        var member = await boardMemberRepository.GetAsync(card.BoardId, userId);
        if (member is null || member.AccessLevel == AccessLevel.Viewer)
            throw new UnauthorizedException("You do not have permission to modify this board.");
        
        var column = card.Column; 
        
        if (card.ColumnId == newColumnId)
        {
            column.MoveCard(card, newPosition);
        }
        else
        {
            var targetColumn = await columnRepository.GetById(newColumnId);
            if (targetColumn is null)
                throw new NotFoundException("Target column not found.");
            
            if (targetColumn.BoardId != card.BoardId)
                throw new ConflictException("Target column belongs to a different board.");
            
            column.RemoveCard(card);
            targetColumn.InsertCard(card, newPosition);
        }
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardMoved(new CardMovedEvent(cardId, newColumnId, newPosition));
    }

    public async Task Delete(Guid userId, Guid cardId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found.");
        
        var member = await boardMemberRepository.GetAsync(card.BoardId, userId);
        if (member is null || member.AccessLevel == AccessLevel.Viewer)
            throw new UnauthorizedException("You do not have permission to modify this board.");
        
        card.Column.RemoveCard(card);

        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardDeleted(new CardDeletedEvent(cardId));
        
        await columnRepository.SaveChangesAsync();
    }

    public async Task<CardDto> GetById(Guid userId, Guid cardId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found.");
        
        var hasReadAccess = await boardRepository.UserHasReadAccess(card.BoardId, userId);
        if (!hasReadAccess)
            throw new UnauthorizedException("You do not have permission to view this card.");

        return card.ToDto();
    }
}