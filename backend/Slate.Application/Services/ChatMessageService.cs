using Microsoft.AspNetCore.SignalR;
using Slate.Application.Events;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class ChatMessageService(
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IChatMessageService
{
    public async Task SendMessageChunkAsync(Guid boardId, Guid messageId, string chunk)
    {
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnMessageChunk(new MessageChunkEvent(boardId, messageId, chunk));
    }

    public async Task SendMessageCompletedAsync(Guid boardId, Guid messageId)
    {
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnMessageCompleted(new MessageCompletedEvent(boardId, messageId));
    }
}