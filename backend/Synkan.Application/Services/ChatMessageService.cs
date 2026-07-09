using Microsoft.AspNetCore.SignalR;
using Synkan.Application.Events;
using Synkan.Application.Hubs;
using Synkan.Application.Interfaces;

namespace Synkan.Application.Services;

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

    public async Task SendProcessingFailedAsync(Guid boardId)
    {
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnProcessingFailed(new ProcessingFailedEvent(boardId));
    }

    public async Task SendProcessingStartedAsync(Guid boardId)
    {
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnProcessingStarted(new ProcessingStartedEvent(boardId));
    }

    public async Task SendProcessingCompletedAsync(Guid boardId)
    {
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnProcessingCompleted(new ProcessingCompletedEvent(boardId));
    }
}