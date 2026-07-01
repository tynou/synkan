using Microsoft.AspNetCore.SignalR;
using Slate.Application.Dto.Response;
using Slate.Application.Events;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Exceptions;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class ColumnService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IHubContext<BoardHub, IBoardClient> hubContext
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
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnCreated(column.ToDto());
        
        return column.Id;
    }

    public async Task UpdateTitle(Guid userId, Guid columnId, string newTitle)
    {
        var column = await columnRepository.GetByIdWithBoard(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");
        
        if (!column.Board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to modify this column.");
        
        column.SetTitle(newTitle);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnTitleUpdated(new ColumnTitleUpdatedEvent(columnId, newTitle));
    }
    
    public async Task Move(Guid userId, Guid columnId, int newPosition)
    {
        var column = await columnRepository.GetByIdWithBoard(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");
        
        if (!column.Board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to modify this column.");
        
        column.Board.MoveColumn(columnId, newPosition);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnMoved(new ColumnMovedEvent(columnId, newPosition));
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
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnDeleted(new ColumnDeletedEvent(columnId));
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