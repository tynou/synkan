using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IBoardService
{
    Task<Guid> Create(Guid userId, string title);

    Task AddMember(Guid userId, Guid boardId, Guid memberId);
    
    Task RemoveMember(Guid userId, Guid boardId, Guid memberId);
    
    Task Update(Guid userId, Guid boardId, string newTitle);
    
    Task Delete(Guid userId, Guid boardId);
    
    Task<BoardDto?> GetById(Guid boardId);
    
    Task<List<BoardLookupDto>> GetBoardsByUserId(Guid userId);
}