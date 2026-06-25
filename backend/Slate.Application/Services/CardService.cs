using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
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

    public async Task Delete(Guid userId, Guid cardId)
    {
        throw new NotImplementedException();
    }

    public async Task<CardDto?> GetById(Guid cardId)
    {
        throw new NotImplementedException();
    }
}