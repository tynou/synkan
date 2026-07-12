using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class SettingsRepository(AppDbContext context) : ISettingsRepository
{
    public async Task<BoardAiSettings?> GetByBoardIdAsync(Guid boardId)
    {
        return await context.BoardAiSettings
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.BoardId == boardId);
    }

    public async Task<BoardAiSettings> UpsertAsync(BoardAiSettings settings)
    {
        var exists = await context.BoardAiSettings.AnyAsync(s => s.BoardId == settings.BoardId);

        if (exists)
            context.BoardAiSettings.Update(settings);
        else
            context.BoardAiSettings.Add(settings);

        return settings;
    }
}