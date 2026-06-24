using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class BoardService(IBoardRepository boardRepository, IUserRepository userRepository) : IBoardService
{
    public async Task<Guid> Create(Guid userId, string title)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new Exception("User not found.");
        
        var board = new Board(user, title);
        await boardRepository.Create(board);
        return board.Id;
    }

    public Task AddMember(Guid userId, Guid memberId, Guid boardId)
    {
        throw new NotImplementedException();
    }

    public async Task<BoardDto?> GetById(Guid id)
    {
        var board = await boardRepository.GetById(id);
        if (board is null)
            throw new Exception("Board not found."); // TODO: make a custom exception
        return board.ToDto();
    }
}