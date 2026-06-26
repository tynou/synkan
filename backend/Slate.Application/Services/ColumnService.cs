using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
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

    public async Task Edit(Guid userId, Guid columnId, string newTitle)
    {
        var column = await columnRepository.GetById(columnId);
        if (column is null)
            throw new Exception("Column not found.");
        
        var hasAccess = await boardRepository.UserHasAccess(userId, column.BoardId);
        if (!hasAccess)
            throw new Exception("You do not have permission to modify this column.");
        
        column.SetTitle(newTitle);
        
        await columnRepository.SaveChangesAsync();
    }

    public async Task Delete(Guid userId, Guid columnId)
    {
        var column = await columnRepository.GetById(columnId);
        if (column is null)
            throw new Exception("Column not found.");
        
        var board = await boardRepository.GetById(column.BoardId);
        if (board is null)
            throw new Exception("Board not found.");
        
        if (!board.UserHasAccess(userId))
            throw new Exception("You do not have permission to delete this column.");
        
        board.RemoveColumn(columnId);
        
        await columnRepository.SaveChangesAsync();
    }

    public async Task<ColumnDto?> GetById(Guid columnId)
    {
        throw new NotImplementedException();
    }
}