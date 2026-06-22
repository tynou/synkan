using Slate.Application.Interfaces;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Application.Services;

public class BoardService(IBoardRepository boardRepository) : IBoardService
{
    public async Task<Guid> Create(Guid userId, string title)
    {
        var board = new Board(Guid.NewGuid(), userId, title);
        await boardRepository.Create(board);
        return board.Id;
    }

    public async Task<Board?> GetById(Guid id)
    {
        return await boardRepository.GetById(id);
    }
}