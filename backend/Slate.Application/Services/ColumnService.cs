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
    public async Task<Guid> Create(Guid userId, Guid boardId, string title)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new Exception("Board not found.");

        if (!board.UserHasAccess(userId))
            throw new Exception("You do not have permission to modify this board.");

        var column = board.AddColumn(title);

        await boardRepository.SaveChangesAsync();
        
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