using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface ILabelRepository
{
    Task<Label?> GetById(Guid labelId);
}