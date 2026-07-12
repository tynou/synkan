using Synkan.Domain.Entities;
using Synkan.Domain.Enums;

namespace Synkan.Application.Interfaces;

public interface ISettingsService
{
    Task<BoardAiSettings> GetOrCreateAsync(Guid boardId);
    
    Task UpdateAsync(Guid boardId, string apiKey, AiProvider provider, string model);
}