using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class LabelRepository(AppDbContext context) : ILabelRepository
{
    public async Task<Label?> GetById(Guid labelId)
    {
        return await context.Labels.FirstOrDefaultAsync(l => l.Id == labelId);
    }
}