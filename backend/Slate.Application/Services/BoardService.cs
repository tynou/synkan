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

public class BoardService(
    IBoardRepository boardRepository,
    IUserRepository userRepository,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IBoardService
{
    public async Task<Guid> Create(Guid userId, bool isPublic, string title)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User not found.");
        
        var board = new Board(userId, isPublic, title);
        await boardRepository.Create(board);
        return board.Id;
    }

    public async Task AddMember(Guid userId, Guid boardId, Guid memberId)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to add members to this board.");
        
        var member = await userRepository.GetByIdAsync(memberId);
        if (member is null)
            throw new NotFoundException("Member user not found.");
        
        board.AddMember(member.Id, AccessLevel.Viewer);

        await boardRepository.SaveChangesAsync();
    }

    public async Task RemoveMember(Guid userId, Guid boardId, Guid memberId)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to remove members from this board.");
        
        board.RemoveMember(memberId);
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task UpdateMemberAccessLevel(Guid userId, Guid boardId, Guid memberId, AccessLevel newAccessLevel)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to change board member access levels.");
        
        board.SetMemberAccessLevel(memberId, newAccessLevel);
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task UpdateTitle(Guid userId, Guid boardId, string newTitle)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to modify this board.");
        
        board.SetTitle(newTitle);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnBoardTitleUpdated(new BoardTitleUpdatedEvent(boardId, newTitle));
    }
    
    public async Task ChangeVisibility(Guid userId, Guid boardId, bool newIsPublic)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to modify this board.");
        
        board.SetVisibility(newIsPublic);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnBoardVisibilityChanged(new BoardVisibilityChangedEvent(boardId, newIsPublic));
    }

    public async Task Delete(Guid userId, Guid boardId)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");

        if (!board.UserHasWriteAccess(userId))
            throw new UnauthorizedException("You do not have permission to delete this board.");

        await boardRepository.Delete(boardId);
        
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnBoardDeleted(new BoardDeletedEvent(boardId));
    }

    public async Task<BoardDto> GetById(Guid userId, Guid boardId)
    {
        var board = await boardRepository.GetById(boardId);
        if (board is null)
            throw new NotFoundException("Board not found.");
        
        if (!board.UserHasReadAccess(userId))
            throw new UnauthorizedException("You do not have permission to read this board.");
        
        return board.ToDto();
    }

    public async Task<List<BoardLookupDto>> GetBoardsByUserId(Guid userId)
    {
        var boards = await boardRepository.GetBoardsByUserId(userId);
        return boards.Select(b => b.ToLookupDto()).ToList();
    }
}