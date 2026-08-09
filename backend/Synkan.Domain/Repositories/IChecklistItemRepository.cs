using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IChecklistItemRepository
{
    Task Add(ChecklistItem checklistItem);
}