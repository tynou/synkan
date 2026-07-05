using Microsoft.EntityFrameworkCore;
using Slate.Domain.Entities;
using Slate.Domain.Repositories;

namespace Slate.Infrastructure.Persistence.Repositories;

public class ChatMessageRepository(AppDbContext context) : IChatMessageRepository
{
    public async Task AddAsync(ChatMessage message)
    {
        await context.ChatMessages.AddAsync(message);
        await context.SaveChangesAsync();
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