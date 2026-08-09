using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface ILabelRepository
{
    Task Add(Label label);
    
    Task<Label?> GetById(Guid labelId);
}