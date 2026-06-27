using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class CardService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    ICardRepository cardRepository
    ) : ICardService
{
    public async Task<Guid> Create(Guid userId, Guid boardId, Guid columnId, string title)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new Exception("Board not found.");
        
        if (!board.UserHasAccess(userId))
            throw new Exception("No access to this board.");
        
        var column = board.Columns.FirstOrDefault(c => c.Id == columnId);
        if (column is null)
            throw new Exception("Column not found on this board.");
        
        var card = column.AddCard(title);
        
        await boardRepository.SaveChangesAsync();

        return card.Id;
    }

    public async Task Update(Guid userId, Guid cardId, string newTitle, string newDescription, Guid newColumnId, int newPosition)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new Exception("Card not found.");
        
        var column = await columnRepository.GetById(card.ColumnId);
        if (column is null)
            throw new Exception("Column not found.");
        
        var board = await boardRepository.GetById(column.BoardId);
        if (board is null)
            throw new Exception("Board not found.");
        
        if (!board.UserHasAccess(userId))
            throw new Exception("You do not have permission to delete this card.");
        
        card.SetTitle(newTitle);
        card.SetDescription(newDescription);
        
        if (card.ColumnId == newColumnId)
        {
            column.MoveCard(card, newPosition);
        }
        else
        {
            var targetColumn = await columnRepository.GetById(newColumnId);
            if (targetColumn is null)
                throw new Exception("Target column not found.");
            
            if (targetColumn.BoardId != board.Id)
                throw new Exception("Target column belongs to a different board.");
            
            column.RemoveCard(card);
            targetColumn.InsertCard(card, newPosition);
        }
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task Delete(Guid userId, Guid cardId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new Exception("Card not found.");
        
        var column = await columnRepository.GetById(card.ColumnId);
        if (column is null)
            throw new Exception("Column not found.");
        
        var board = await boardRepository.GetById(column.BoardId);
        if (board is null)
            throw new Exception("Board not found.");
        
        if (!board.UserHasAccess(userId))
            throw new Exception("You do not have permission to delete this card.");
        
        column.RemoveCard(card);
        
        await cardRepository.SaveChangesAsync();
    }

    public async Task<CardDto?> GetById(Guid cardId)
    {
        var card = await cardRepository.GetById(cardId);
        if (card is null)
            throw new Exception("Card not found.");

        return card.ToDto();
    }
}