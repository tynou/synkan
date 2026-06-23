using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class ColumnService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository
    ) : IColumnService
{
    public async Task<Guid> Create(Guid userId, Guid boardId)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new Exception("Board not found.");

        if (board.OwnerId != userId)
            throw new Exception("You do not have permission to modify this board.");
        
        var column = new Column(Guid.NewGuid(), boardId);
        await columnRepository.Create(column);
        
        return column.Id;
    }

    public async Task Delete(Guid userId, Guid columnId)
    {
        throw new NotImplementedException();
    }

    public async Task<ColumnDto?> GetById(Guid columnId)
    {
        throw new NotImplementedException();
    }
}