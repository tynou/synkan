using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface ILabelRepository
{
    Task<Label?> GetById(Guid labelId);
}