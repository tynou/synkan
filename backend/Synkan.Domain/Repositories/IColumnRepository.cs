using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IColumnRepository
{
    Task Add(Column column);
    
    Task<Column?> GetById(Guid columnId);
}