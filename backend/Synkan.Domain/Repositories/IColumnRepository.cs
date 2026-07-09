using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IColumnRepository
{
    Task Create(Column column);

    Task Delete(Guid columnId);
    
    Task<Column?> GetById(Guid columnId);
    
    Task<Column?> GetByIdWithBoard(Guid columnId);
    
    Task SaveChangesAsync();
}