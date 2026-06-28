using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IColumnService
{
    Task<Guid> Create(Guid userId, Guid boardId,  string title);
    
    Task Update(Guid userId, Guid columnId, string newTitle, int newPosition);
    
    Task Delete(Guid userId, Guid columnId);
    
    Task<ColumnDto?> GetById(Guid userId, Guid columnId);
}