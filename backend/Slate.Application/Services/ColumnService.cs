using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Exceptions;
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
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to modify this board.");

        var column = board.AddColumn(title);

        await boardRepository.SaveChangesAsync();
        
        return column.Id;
    }

    public async Task Update(Guid userId, Guid columnId, string newTitle, int newPosition)
    {
        var column = await columnRepository.GetByIdWithBoard(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");
        
        if (!column.Board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to modify this column.");
        
        column.SetTitle(newTitle);
        
        column.Board.MoveColumn(columnId, newPosition);
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task Delete(Guid userId, Guid columnId)
    {
        var column = await columnRepository.GetByIdWithBoard(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");
        
        if (!column.Board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to delete this column.");
        
        column.Board.RemoveColumn(columnId);
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task<ColumnDto> GetById(Guid userId, Guid columnId)
    {
        var column = await columnRepository.GetByIdWithBoard(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");
        
        if (!column.Board.UserHasReadAccess(userId))
            throw new UnauthorizedException("You do not have permission to view this column.");

        return column.ToDto();
    }
}