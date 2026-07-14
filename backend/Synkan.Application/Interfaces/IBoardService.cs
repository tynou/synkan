using Synkan.Application.Dto.Response;
using Synkan.Domain.Enums;

namespace Synkan.Application.Interfaces;

public interface IBoardService
{
    Task<Guid> Create(bool isPublic, string title);
    
    Task<Guid> CreateLabel(Guid boardId, string name, string color);

    Task AddMember(Guid boardId, Guid memberId);
    
    Task RemoveMember(Guid boardId, Guid memberId);
    
    Task UpdateMemberAccessLevel(Guid boardId, Guid memberId, AccessLevel newAccessLevel);
    
    Task UpdateTitle(Guid boardId, string newTitle);

    Task ChangeVisibility(Guid boardId, bool newIsPublic);
    
    Task Delete(Guid boardId);
    
    Task<BoardDto> GetById(Guid boardId);
    
    Task<List<BoardLookupDto>> GetBoardsByUserId(Guid userId);
}