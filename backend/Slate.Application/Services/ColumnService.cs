using Microsoft.AspNetCore.SignalR;
using Slate.Application.Dto.Response;
using Slate.Application.Events;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Entities;
using Slate.Domain.Enums;
using Slate.Domain.Exceptions;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class ColumnService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardMemberRepository boardMemberRepository,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IColumnService
{
    public async Task<Guid> Create(Guid userId, Guid boardId, string title)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Member);

        var column = board.AddColumn(title);

        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnCreated(column.ToDto());
        
        return column.Id;
    }

    public async Task UpdateTitle(Guid userId, Guid columnId, string newTitle)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        column.SetTitle(newTitle);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnTitleUpdated(new ColumnTitleUpdatedEvent(columnId, newTitle));
    }
    
    public async Task Move(Guid userId, Guid columnId, int newPosition)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        column.Board.MoveColumn(columnId, newPosition);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnMoved(new ColumnMovedEvent(columnId, newPosition));
    }

    public async Task Delete(Guid userId, Guid columnId)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        column.Board.RemoveColumn(columnId);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnDeleted(new ColumnDeletedEvent(columnId));
    }

    public async Task<ColumnDto> GetById(Guid userId, Guid columnId)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Viewer);
        return column.ToDto();
    }
    
    private async Task<Column> GetColumnAndVerifyAccess(Guid columnId, Guid userId, AccessLevel minRequiredLevel)
    {
        var column = await columnRepository.GetById(columnId);
        if (column is null)
            throw new NotFoundException("Column not found.");

        await VerifyBoardAccess(column.BoardId, userId, minRequiredLevel);
        return column;
    }
    
    private async Task<Board> GetBoardAndVerifyAccess(Guid boardId, Guid userId, AccessLevel minRequiredLevel)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        await VerifyBoardAccess(boardId, userId, minRequiredLevel);
        return board;
    }

    private async Task VerifyBoardAccess(Guid boardId, Guid userId, AccessLevel minRequiredLevel)
    {
        var member = await boardMemberRepository.GetAsync(boardId, userId);

        if (member is null || member.AccessLevel < minRequiredLevel)
            throw new UnauthorizedException("You do not have permission to access this board.");
    }
}