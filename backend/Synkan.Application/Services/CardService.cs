using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Synkan.Application.Dto.Response;
using Synkan.Application.Events;
using Synkan.Application.Hubs;
using Synkan.Application.Interfaces;
using Synkan.Application.Mappers;
using Synkan.Domain.Entities;
using Synkan.Domain.Enums;
using Synkan.Domain.Exceptions;
using Synkan.Domain.Repositories;

namespace Synkan.Application.Services;

public class CardService(
    IColumnRepository columnRepository,
    ICardRepository cardRepository,
    IBoardMemberRepository boardMemberRepository,
    ILabelRepository labelRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : ICardService
{
    private Guid UserId => currentUser.UserId;
    
    public async Task<Guid> Create(Guid userId, Guid columnId, string title)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        
        var card = column.AddCard(title);
        
        await unitOfWork.SaveChangesAsync();

        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardCreated(card.ToDto());

        return card.Id;
    }

    public async Task UpdateContent(Guid userId, Guid cardId, string newTitle, string newDescription)
    {
        var card = await GetCardAndVerifyAccess(cardId, userId, AccessLevel.Member);
        
        card.SetTitle(newTitle);
        card.SetDescription(newDescription);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardContentUpdated(new CardContentUpdatedEvent(cardId, newTitle, newDescription));
    }
    
    public async Task UpdateCover(Guid userId, Guid cardId, string? color)
    {
        var card = await GetCardAndVerifyAccess(cardId, userId, AccessLevel.Member);
        card.UpdateCoverColor(color);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardCoverUpdated(new CardCoverUpdatedEvent(cardId, color));
    }

    public async Task UpdateDueDate(Guid cardId, DateTimeOffset dueDate, DateTimeOffset reminderTime)
    {
        var card = await GetCardAndVerifyAccess(cardId, UserId, AccessLevel.Member);
        
        if (!string.IsNullOrEmpty(card.ReminderJobId))
            BackgroundJob.Delete(card.ReminderJobId);
        
        if (reminderTime < DateTimeOffset.UtcNow)
            return;
        
        var newJobId = BackgroundJob.Schedule<INotificationService>(
            service => service.SendDeadlineReminder(), 
            reminderTime
        );
        
        card.UpdateDeadline(dueDate, reminderTime, newJobId);
        
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveDueDate(Guid cardId)
    {
        var card = await GetCardAndVerifyAccess(cardId, UserId, AccessLevel.Member);
        
        if (!string.IsNullOrEmpty(card.ReminderJobId))
            BackgroundJob.Delete(card.ReminderJobId);
        
        card.RemoveDeadline();
        
        await unitOfWork.SaveChangesAsync();
    }

    public async Task AssignLabel(Guid cardId, Guid labelId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        var label = await labelRepository.GetById(labelId);
        if (label is null)
            throw new NotFoundException("Label not found");
        
        card.AssignLabel(label);
        
        await unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveLabel(Guid cardId, Guid labelId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found");
        
        card.RemoveLabel(labelId);

        await unitOfWork.SaveChangesAsync();
    }

    public async Task Move(Guid userId, Guid cardId, Guid newColumnId, int newPosition)
    {
        var card = await GetCardAndVerifyAccess(cardId, userId, AccessLevel.Member);
        
        var column = card.Column; 
        
        if (card.ColumnId == newColumnId)
        {
            column.MoveCard(card, newPosition);
        }
        else
        {
            var targetColumn = await GetColumnAndVerifyAccess(newColumnId, userId, AccessLevel.Member);
            
            if (targetColumn.BoardId != card.BoardId)
                throw new ConflictException("Target column belongs to a different board.");
            
            column.RemoveCard(card);
            targetColumn.InsertCard(card, newPosition);
        }
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardMoved(new CardMovedEvent(cardId, newColumnId, newPosition));
    }

    public async Task Delete(Guid userId, Guid cardId)
    {
        var card = await GetCardAndVerifyAccess(cardId, userId, AccessLevel.Member);
        card.Column.RemoveCard(card);

        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(card.BoardId.ToString())
            .OnCardDeleted(new CardDeletedEvent(cardId));
    }

    public async Task<CardDto> GetById(Guid userId, Guid cardId)
    {
        var card = await GetCardAndVerifyAccess(cardId, userId, AccessLevel.Viewer);
        return card.ToDto();
    }
    
    private async Task<Card> GetCardAndVerifyAccess(Guid cardId, Guid userId, AccessLevel minRequiredLevel)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new NotFoundException("Card not found.");

        await VerifyBoardAccess(card.BoardId, userId, minRequiredLevel);
        return card;
    }

    private async Task<Column> GetColumnAndVerifyAccess(Guid columnId, Guid userId, AccessLevel minRequiredLevel)
    {
        var column = await columnRepository.GetById(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");

        await VerifyBoardAccess(column.BoardId, userId, minRequiredLevel);
        return column;
    }

    private async Task VerifyBoardAccess(Guid boardId, Guid userId, AccessLevel minRequiredLevel)
    {
        var member = await boardMemberRepository.GetAsync(boardId, userId);

        if (member is null || member.AccessLevel < minRequiredLevel)
            throw new UnauthorizedException("You do not have permission to access this board.");
    }
}