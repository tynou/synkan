using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IColumnRepository
{
    Task<Column?> GetById(Guid columnId);
}