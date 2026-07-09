using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class LabelRepository(AppDbContext context) : ILabelRepository
{
    public async Task<Label?> GetById(Guid labelId)
    {
        return await context.Labels.FirstOrDefaultAsync(l => l.Id == labelId);
    }
}