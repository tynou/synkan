namespace Slate.Application.Interfaces;

public interface IChatMessageService
{
    public Task SendMessageChunkAsync(Guid boardId, Guid messageId, string chunk);
    
    public Task SendMessageCompletedAsync(Guid boardId, Guid messageId);
}