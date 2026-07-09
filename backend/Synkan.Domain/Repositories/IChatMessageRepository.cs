using Synkan.Domain.Entities;

namespace Synkan.Domain.Repositories;

public interface IChatMessageRepository
{
    Task AddAsync(ChatMessage message);

    Task<IReadOnlyCollection<ChatMessage>> GetByBoardIdAsync(Guid boardId);
}