using Slate.Domain.Entities;

namespace Slate.Domain.Repositories;

public interface IChatMessageRepository
{
    Task AddAsync(ChatMessage message);

    Task<IReadOnlyCollection<ChatMessage>> GetByBoardIdAsync(Guid boardId);
}