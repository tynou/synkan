using Slate.Application.Dto.Response;
using Slate.Domain.Enums;

namespace Slate.Application.Interfaces;

public interface IBoardService
{
    Task<Guid> Create(Guid userId, bool isPublic, string title);

    Task AddMember(Guid userId, Guid boardId, Guid memberId);
    
    Task RemoveMember(Guid userId, Guid boardId, Guid memberId);
    
    Task UpdateMemberAccessLevel(Guid userId, Guid boardId, Guid memberId, AccessLevel newAccessLevel);
    
    Task Update(Guid userId, Guid boardId, bool newIsPublic, string newTitle);
    
    Task Delete(Guid userId, Guid boardId);
    
    Task<BoardDto?> GetById(Guid userId, Guid boardId);
    
    Task<List<BoardLookupDto>> GetBoardsByUserId(Guid userId);
}