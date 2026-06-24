using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IBoardService
{
    Task<Guid> Create(Guid userId, string title);

    Task AddMember(Guid userId, Guid memberId, Guid boardId);
    
    Task<BoardDto?> GetById(Guid id);
}