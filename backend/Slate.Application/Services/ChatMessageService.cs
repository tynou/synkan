using Microsoft.AspNetCore.SignalR;
using Slate.Application.Events;
using Slate.Application.Hubs;
using Slate.Application.Interfaces;

namespace Slate.Application.Services;

public class ChatMessageService(
    IHubContext<BoardHub, IBoardClient> hubContext
    ) : IChatMessageService
{
    public async Task SendMessageChunkAsync(Guid boardId, string chunk)
    {
        await hubContext.Clients
            .Group(boardId.ToString())
            .OnMessageChunk(new MessageChunkEvent(boardId, chunk));
    }
}