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
    IBoardMemberRepository boardMemberRepository,
    ICurrentUserService currentUser,
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IBoardService
{
    private Guid UserId => currentUser.UserId;
    
    public async Task<Guid> Create(Guid userId, bool isPublic, string title)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException("User not found.");
        
        var board = new Board(userId, isPublic, title);
        await boardRepository.Create(board);
        return board.Id;
    }

    public async Task<Guid> CreateLabel(Guid boardId, string name, string color)
    {
        var board = await GetBoardAndVerifyAccess(boardId, UserId, AccessLevel.Member);
        
        var label = board.CreateLabel(name, color);

        await boardRepository.SaveChangesAsync();

        return label.Id;
    }

    public async Task AddMember(Guid userId, Guid boardId, Guid memberId)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Admin);
        
        var member = await userRepository.GetByIdAsync(memberId);
        if (member is null)
            throw new NotFoundException("Member user not found.");
        
        board.AddMember(memberId, AccessLevel.Viewer);

        await boardRepository.SaveChangesAsync();
    }

    public async Task RemoveMember(Guid userId, Guid boardId, Guid memberId)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Admin);
        board.RemoveMember(memberId);
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task UpdateMemberAccessLevel(Guid userId, Guid boardId, Guid memberId, AccessLevel newAccessLevel)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Admin);
        board.SetMemberAccessLevel(memberId, newAccessLevel);
        
        await boardRepository.SaveChangesAsync();
    }

    public async Task UpdateTitle(Guid userId, Guid boardId, string newTitle)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Admin);
        board.SetTitle(newTitle);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnBoardTitleUpdated(new BoardTitleUpdatedEvent(boardId, newTitle));
    }
    
    public async Task ChangeVisibility(Guid userId, Guid boardId, bool newIsPublic)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Admin);
        board.SetVisibility(newIsPublic);
        
        await boardRepository.SaveChangesAsync();
        
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnBoardVisibilityChanged(new BoardVisibilityChangedEvent(boardId, newIsPublic));
    }

    public async Task Delete(Guid userId, Guid boardId)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Admin);

        await boardRepository.Delete(boardId);
        
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnBoardDeleted(new BoardDeletedEvent(boardId));
    }

    public async Task<BoardDto> GetById(Guid userId, Guid boardId)
    {
        var board = await GetBoardAndVerifyAccess(boardId, userId, AccessLevel.Viewer);
        
        return board.ToDto();
    }

    public async Task<List<BoardLookupDto>> GetBoardsByUserId(Guid userId)
    {
        var boards = await boardRepository.GetBoardsByUserId(userId);
        return boards.Select(b => b.ToLookupDto()).ToList();
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