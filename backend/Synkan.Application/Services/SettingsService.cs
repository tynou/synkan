using Synkan.Application.Interfaces;
using Synkan.Domain.Entities;
using Synkan.Domain.Enums;
using Synkan.Domain.Repositories;

namespace Synkan.Application.Services;

public class SettingsService(
    ISettingsRepository settingsRepository,
    IUnitOfWork unitOfWork
    ) : ISettingsService
{
    public async Task<BoardAiSettings> GetOrCreateAsync(Guid boardId)
    {
        var settings = await settingsRepository.GetByBoardIdAsync(boardId);
        if (settings is not null)
            return settings;

        settings = new BoardAiSettings(boardId);
        
        await settingsRepository.UpsertAsync(settings);
        await unitOfWork.SaveChangesAsync();
        
        return settings;
    }

    public async Task UpdateAsync(Guid boardId, string apiKey, AiProvider provider, string model)
    {
        var settings = await GetOrCreateAsync(boardId);
        
        settings.UpdateSettings(apiKey, provider, model);
        
        await settingsRepository.UpsertAsync(settings);
        await unitOfWork.SaveChangesAsync();
    }
}