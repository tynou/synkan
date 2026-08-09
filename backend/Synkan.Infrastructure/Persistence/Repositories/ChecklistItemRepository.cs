using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class ChecklistItemRepository(AppDbContext context) : IChecklistItemRepository
{
    public async Task Add(ChecklistItem checklistItem)
    {
        context.ChecklistItems.Add(checklistItem);
    }
}