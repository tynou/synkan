using Slate.Application.Dto.Response;

namespace Slate.Application.Interfaces;

public interface IColumnService
{
    Task<Guid> Create(Guid userId, Guid boardId,  string title);
    
    Task Delete(Guid userId, Guid columnId);
    
    Task<ColumnDto?> GetById(Guid columnId);
}