using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class ChecklistRepository(AppDbContext context) : IChecklistRepository
{
    public async Task Add(Checklist checklist)
    {
        context.Checklists.Add(checklist);
    }
}