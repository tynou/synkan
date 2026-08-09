using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IChecklistRepository
{
    Task Add(Checklist checklist);
}