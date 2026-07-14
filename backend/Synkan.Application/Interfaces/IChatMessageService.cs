using Synkan.Domain.Entities;

namespace Synkan.Application.Interfaces;

public interface IChatMessageService
{
    public Task SendMessageSentAsync(Guid boardId, ChatMessage message);
    
    public Task SendMessageChunkAsync(Guid boardId, Guid messageId, string chunk);
    
    public Task SendMessageCompletedAsync(Guid boardId, Guid messageId);
    
    public Task SendProcessingFailedAsync(Guid boardId);
    
    public Task SendProcessingStartedAsync(Guid boardId);
    
    public Task SendProcessingCompletedAsync(Guid boardId);
}