using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface ISettingsRepository
{
    Task<BoardAiSettings?> GetByBoardIdAsync(Guid boardId);
    
    Task<BoardAiSettings> UpsertAsync(BoardAiSettings settings);
}