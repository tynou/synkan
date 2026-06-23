using Slate.Application.Dto.Response;
using Slate.Application.Interfaces;
using Slate.Application.Mappers;
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

    public async Task<BoardDto?> GetById(Guid id)
    {
        var board = await boardRepository.GetById(id);
        if (board is null)
            throw new Exception("Board not found."); // TODO: make a custom exception
        return board.ToDto();
    }
}