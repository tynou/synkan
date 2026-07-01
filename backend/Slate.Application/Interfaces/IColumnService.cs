using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IColumnService
{
    Task<Guid> Create(Guid userId, Guid boardId,  string title);
    
    Task UpdateTitle(Guid userId, Guid columnId, string newTitle);
    
    Task Move(Guid userId, Guid columnId, int newPosition);
    
    Task Delete(Guid userId, Guid columnId);
    
    Task<ColumnDto> GetById(Guid userId, Guid columnId);
}