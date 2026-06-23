using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface IColumnRepository
{
    Task Create(Column column);

    Task Delete(Guid columnId);
    
    Task<Column?> GetById(Guid columnId);
}