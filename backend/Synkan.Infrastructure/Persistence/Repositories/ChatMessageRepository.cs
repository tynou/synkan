using Microsoft.EntityFrameworkCore;
using Synkan.Domain.Entities;
using Synkan.Domain.Repositories;

namespace Synkan.Infrastructure.Persistence.Repositories;

public class ChatMessageRepository(AppDbContext context) : IChatMessageRepository
{
    public async Task AddAsync(ChatMessage message)
    {
        await context.ChatMessages.AddAsync(message);
    }

    public async Task<IReadOnlyCollection<ChatMessage>> GetByBoardIdAsync(Guid boardId)
    {
        return await context.ChatMessages
            .AsNoTracking()
            .Where(m => m.BoardId == boardId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
}