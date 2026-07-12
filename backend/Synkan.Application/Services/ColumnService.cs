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

public class ColumnService(
    IBoardRepository boardRepository,
    IColumnRepository columnRepository,
    IBoardMemberRepository boardMemberRepository,
    IUnitOfWork unitOfWork,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IColumnService
{
    public async Task<Guid> Create(Guid userId, Guid boardId, string title)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Member);

        var column = board.AddColumn(title);

        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnCreated(column.ToDto());
        
        return column.Id;
    }

    public async Task UpdateTitle(Guid userId, Guid columnId, string newTitle)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        column.SetTitle(newTitle);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnTitleUpdated(new ColumnTitleUpdatedEvent(columnId, newTitle));
    }
    
    public async Task Move(Guid userId, Guid columnId, int newPosition)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        column.Board.MoveColumn(columnId, newPosition);
        
        await unitOfWork.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(column.BoardId.ToString())
            .OnColumnMoved(new ColumnMovedEvent(columnId, newPosition));
    }

    public async Task Delete(Guid userId, Guid columnId)
    {
        var column = await GetColumnAndVerifyAccess(columnId, userId, AccessLevel.Member);
        column.Board.RemoveColumn(columnId);
        
        await unitOfWork.SaveChangesAsync();
        
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