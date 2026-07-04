namespace Slate.Application.Interfaces;

public interface IChatMessageService
{
    public Task SendMessageChunkAsync(Guid boardId, string chunk);
}