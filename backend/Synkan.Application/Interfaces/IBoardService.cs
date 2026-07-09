using Synkan.Application.Dto.Response;
using Synkan.Domain.Enums;

namespace Synkan.Application.Interfaces;

public interface IBoardService
{
    Task<Guid> Create(Guid userId, bool isPublic, string title);
    
    Task<Guid> CreateLabel(Guid boardId, string name, string color);

    Task AddMember(Guid userId, Guid boardId, Guid memberId);
    
    Task RemoveMember(Guid userId, Guid boardId, Guid memberId);
    
    Task UpdateMemberAccessLevel(Guid userId, Guid boardId, Guid memberId, AccessLevel newAccessLevel);
    
    Task UpdateTitle(Guid userId, Guid boardId, string newTitle);

    Task ChangeVisibility(Guid userId, Guid boardId, bool newIsPublic);
    
    Task Delete(Guid userId, Guid boardId);
    
    Task<BoardDto> GetById(Guid userId, Guid boardId);
    
    Task<List<BoardLookupDto>> GetBoardsByUserId(Guid userId);
}