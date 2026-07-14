using Synkan.Application.Dto.Response;

namespace Synkan.Application.Interfaces;

public interface IColumnService
{
    Task<Guid> Create(Guid boardId,  string title);
    
    Task UpdateTitle(Guid columnId, string newTitle);
    
    Task Move(Guid columnId, int newPosition);
    
    Task Delete(Guid columnId);
    
    Task<ColumnDto> GetById(Guid columnId);
}