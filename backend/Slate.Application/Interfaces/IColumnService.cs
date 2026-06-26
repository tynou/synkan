using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IColumnService
{
    Task<Guid> Create(Guid userId, Guid boardId,  string title);
    
    Task Edit(Guid userId, Guid columnId, string newTitle);
    
    Task Delete(Guid userId, Guid columnId);
    
    Task<ColumnDto?> GetById(Guid columnId);
}